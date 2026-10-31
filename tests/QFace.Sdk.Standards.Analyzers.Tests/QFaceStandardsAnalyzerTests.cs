using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using QFace.Sdk.Standards.Analyzers;
using Xunit;

namespace QFace.Sdk.Standards.Analyzers.Tests;

public sealed class QFaceStandardsAnalyzerTests
{
    [Theory]
    [InlineData("using QimErp.Payroll.Core; namespace QimErp.Finance.Ar.WebApi { class C {} }", "QFS001")]
    [InlineData("namespace QimErp.Payroll.Core { public class Run {} } namespace QimErp.Finance.Ar.WebApi { class C { public QimErp.Payroll.Core.Run Run { get; set; } = new(); } }", "QFS002")]
    [InlineData("namespace QimErp.Finance.Ar.WebApi { class C { void Handle(Microsoft.EntityFrameworkCore.DbContext db) { db.SaveChanges(); } } } namespace Microsoft.EntityFrameworkCore { class DbContext { public void SaveChanges() {} } }", "QFS003")]
    [InlineData("namespace QimErp.Finance.Ar.WebApi { class C { void Handle(System.Net.Http.HttpClient client) { client.GetStringAsync(\"x\"); } } } namespace System.Net.Http { class HttpClient { public void GetStringAsync(string value) {} } }", "QFS004")]
    [InlineData("namespace QimErp.Finance.Ar.WebApi { class C { void Get() { query.IgnoreQueryFilters(); } Query query = new(); class Query { public void IgnoreQueryFilters() {} } } }", "QFS005")]
    [InlineData("namespace QimErp.Finance.Ar.WebApi { class C { void Get() { query.FromSqlRaw(\"select 1\"); } Query query = new(); class Query { public void FromSqlRaw(string sql) {} } } }", "QFS006")]
    [InlineData("namespace QimErp.Finance.Ar.WebApi { class C { void Get() { logger.LogWarning(\"Payment failed: {Body}\", body); } Logger logger = new(); string body = \"body\"; class Logger { public void LogWarning(string template, string value) {} } } }", "QFS007")]
    public async Task ReportsExpectedViolation(string source, string expectedId)
    {
        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == expectedId);
    }

    [Fact]
    public async Task AllowsSharedContractsAndServiceDelegation()
    {
        const string source = "using QimErp.Shared.Common; namespace QimErp.Finance.Ar.WebApi { class C { void Handle(IService service) { service.Execute(); } } interface IService { void Execute(); } } namespace QimErp.Shared.Common { class Contract {} }";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id is "QFS001" or "QFS003" or "QFS004");
    }

    [Fact]
    public async Task AllowsIdentifierOnlyStructuredLogs()
    {
        const string source = "namespace QimErp.Finance.Ar.WebApi { class C { void Get() { logger.LogWarning(\"Payment failed: {PaymentId}\", id); } Logger logger = new(); class Logger { public void LogWarning(string template, string value) {} } string id = \"id\"; } }";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "QFS007");
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
        var compilation = CSharpCompilation.Create(
            "QimErp.Finance.Ar.WebApi",
            [tree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return await compilation.WithAnalyzers([new QFaceStandardsAnalyzer()]).GetAnalyzerDiagnosticsAsync();
    }
}
