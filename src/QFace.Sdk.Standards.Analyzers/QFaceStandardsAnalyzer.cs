using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace QFace.Sdk.Standards.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class QFaceStandardsAnalyzer : DiagnosticAnalyzer
{
    private const string Category = "QFace.Architecture";

    private static readonly DiagnosticDescriptor CrossModule = Rule(
        "QFS001", "Cross-module dependency", "{0} crosses module boundary from '{1}' to '{2}'", "DATA-01");
    private static readonly DiagnosticDescriptor CrossModuleRelationship = Rule(
        "QFS002", "Cross-module entity relationship", "{0} creates a cross-module relationship from '{1}' to '{2}'", "DATA-04");
    private static readonly DiagnosticDescriptor HandlerDbContext = Rule(
        "QFS003", "Handler directly uses DbContext", "Request handlers must delegate database access to one application service", "DATA-15");
    private static readonly DiagnosticDescriptor HandlerHttpClient = Rule(
        "QFS004", "Handler directly uses HttpClient", "Request handlers must not synchronously call downstream services", "DATA-02");
    private static readonly DiagnosticDescriptor IgnoreQueryFilters = Rule(
        "QFS005", "Tenant filter bypass", "IgnoreQueryFilters bypasses tenant isolation and requires a reviewed tenant-scoped repository operation", "DATA-20");
    private static readonly DiagnosticDescriptor RawSql = Rule(
        "QFS006", "Raw SQL requires tenant review", "{0} bypasses query-shape protections; use a tenant-scoped repository operation or an explicitly reviewed query", "DATA-20");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [CrossModule, CrossModuleRelationship, HandlerDbContext, HandlerHttpClient, IgnoreQueryFilters, RawSql];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var module = ModuleBoundary.FromAssemblyName(start.Compilation.AssemblyName);
            if (module is null)
                return;

            start.RegisterCompilationEndAction(end => AnalyzeReferences(end, module));
            start.RegisterSyntaxNodeAction(node => AnalyzeUsing(node, module), Microsoft.CodeAnalysis.CSharp.SyntaxKind.UsingDirective);
            start.RegisterSymbolAction(symbol => AnalyzeProperty(symbol, module), SymbolKind.Property);
            start.RegisterSyntaxNodeAction(node => AnalyzeInvocation(node, module), Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
        });
    }

    private static DiagnosticDescriptor Rule(string id, string title, string message, string helpRule) =>
        new(id, title, message, Category, DiagnosticSeverity.Error, true,
            helpLinkUri: $"https://mcp.qfacesolutions.com/standards/{helpRule}");

    private static void AnalyzeReferences(CompilationAnalysisContext context, ModuleBoundary module)
    {
        var location = context.Compilation.SyntaxTrees.FirstOrDefault()?.GetRoot(context.CancellationToken).GetLocation();
        if (location is null)
            return;

        foreach (var reference in context.Compilation.References)
        {
            var target = context.Compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            var targetModule = ModuleBoundary.FromAssemblyName(target?.Name);
            if (targetModule is not null && !module.Equals(targetModule))
                context.ReportDiagnostic(Diagnostic.Create(CrossModule, location, "Assembly reference", module, targetModule));
        }
    }

    private static void AnalyzeUsing(SyntaxNodeAnalysisContext context, ModuleBoundary module)
    {
        var directive = (UsingDirectiveSyntax)context.Node;
        var target = ModuleBoundary.FromNamespace(directive.Name?.ToString());
        if (target is not null && !module.Equals(target))
            context.ReportDiagnostic(Diagnostic.Create(CrossModule, directive.GetLocation(), "Using directive", module, target));
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, ModuleBoundary module)
    {
        var property = (IPropertySymbol)context.Symbol;
        if (property.IsStatic || property.DeclaredAccessibility != Accessibility.Public)
            return;

        var owner = ModuleBoundary.FromNamespace(property.ContainingType.ContainingNamespace.ToDisplayString()) ?? module;
        var related = FindForeignModule(property.Type, owner);
        if (related is not null)
            Report(context, CrossModuleRelationship, property.Locations.FirstOrDefault(), "Navigation property", owner, related);
    }

    private static ModuleBoundary? FindForeignModule(ITypeSymbol type, ModuleBoundary owner)
    {
        if (type is INamedTypeSymbol named)
        {
            var boundary = ModuleBoundary.FromNamespace(named.ContainingNamespace.ToDisplayString());
            if (boundary is not null && !owner.Equals(boundary))
                return boundary;

            foreach (var argument in named.TypeArguments)
            {
                var nested = FindForeignModule(argument, owner);
                if (nested is not null)
                    return nested;
            }
        }

        return null;
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, ModuleBoundary module)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var method = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
        if (method is null)
            return;

        if (method.Name == "IgnoreQueryFilters")
            context.ReportDiagnostic(Diagnostic.Create(IgnoreQueryFilters, invocation.GetLocation()));

        if (method.Name is "FromSqlRaw" or "ExecuteSqlRaw" or "ExecuteSqlRawAsync" or "SqlQueryRaw")
            context.ReportDiagnostic(Diagnostic.Create(RawSql, invocation.GetLocation(), method.Name));

        if (method.Name is "HasForeignKey" or "HasOne" && method.TypeArguments.Length > 0)
        {
            var owner = ModuleBoundary.FromNamespace(context.ContainingSymbol?.ContainingType?.ContainingNamespace.ToDisplayString()) ?? module;
            var related = FindForeignModule(method.TypeArguments[0], owner);
            if (related is not null)
                context.ReportDiagnostic(Diagnostic.Create(CrossModuleRelationship, invocation.GetLocation(), "Entity configuration", owner, related));
        }

        if (IsInsideRequestHandler(invocation) && IsDbContextOrHttpClientUse(context.SemanticModel, invocation, method, context.CancellationToken))
        {
            var descriptor = IsHttpClientUse(context.SemanticModel, invocation, method, context.CancellationToken)
                ? HandlerHttpClient
                : HandlerDbContext;
            context.ReportDiagnostic(Diagnostic.Create(descriptor, invocation.GetLocation()));
        }
    }

    private static bool IsDbContextOrHttpClientUse(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        IMethodSymbol method,
        System.Threading.CancellationToken cancellationToken)
    {
        var receiver = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
        var receiverType = receiver is null ? null : semanticModel.GetTypeInfo(receiver, cancellationToken).Type;
        return IsDbContext(method.ContainingType) || IsHttpClient(method.ContainingType) ||
            IsDbContext(receiverType) || IsHttpClient(receiverType);
    }

    private static bool IsHttpClientUse(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        IMethodSymbol method,
        System.Threading.CancellationToken cancellationToken)
    {
        var receiver = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
        var receiverType = receiver is null ? null : semanticModel.GetTypeInfo(receiver, cancellationToken).Type;
        return IsHttpClient(method.ContainingType) || IsHttpClient(receiverType);
    }

    private static bool IsDbContext(ITypeSymbol? type) => IsOrBase(type, "DbContext", "Microsoft.EntityFrameworkCore");
    private static bool IsHttpClient(ITypeSymbol? type) => IsOrBase(type, "HttpClient", "System.Net.Http");

    private static bool IsOrBase(ITypeSymbol? type, string name, string namespaceName)
    {
        for (var current = type; current is not null; current = current.BaseType)
            if (current.Name == name && current.ContainingNamespace.ToDisplayString() == namespaceName)
                return true;
        return false;
    }

    private static bool IsInsideRequestHandler(SyntaxNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (current is AnonymousFunctionExpressionSyntax lambda &&
                lambda.Parent is ArgumentSyntax argument && argument.Parent?.Parent is InvocationExpressionSyntax map &&
                map.Expression.ToString().StartsWith("app.Map", StringComparison.Ordinal))
                return true;

            if (current is MethodDeclarationSyntax method && method.Identifier.ValueText.StartsWith("Handle", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, Location? location, params object[] arguments)
    {
        if (location is not null)
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, arguments));
    }

    private sealed class ModuleBoundary : IEquatable<ModuleBoundary>
    {
        private ModuleBoundary(string value) => Value = value;
        public string Value { get; }

        public static ModuleBoundary? FromAssemblyName(string? name) => FromParts(name?.Split('.'));
        public static ModuleBoundary? FromNamespace(string? name) => FromParts(name?.Split('.'));

        private static ModuleBoundary? FromParts(string[]? parts)
        {
            if (parts is null || parts.Length < 3 || !string.Equals(parts[0], "QimErp", StringComparison.Ordinal) ||
                string.Equals(parts[1], "Shared", StringComparison.Ordinal))
                return null;
            return new ModuleBoundary($"{parts[0]}.{parts[1]}.{parts[2]}");
        }

        public bool Equals(ModuleBoundary? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => Equals(obj as ModuleBoundary);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
    }
}
