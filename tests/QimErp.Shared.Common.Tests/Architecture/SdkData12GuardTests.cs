using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-12: SDK <c>WorkflowStarter</c> fail-closed rethrow on start failures
/// (WorkflowAlreadyStartedException excluded). Named residuals are marked on the
/// source surfaces below (see monorepo DECISIONS 2026-10-20 for narrative).
/// </summary>
public class SdkData12GuardTests
{
    private const string Data12MarkerPrefix = "// DATA-12:";

    [Fact]
    public void WorkflowStarter_StartOrIgnoreAsync_rethrows_on_non_already_started_failures()
    {
        AssertGeneralExceptionCatchRethrows("StartOrIgnoreAsync");
    }

    [Fact]
    public void WorkflowStarter_StartOrRaiseAsync_rethrows_on_failures()
    {
        AssertGeneralExceptionCatchRethrows("StartOrRaiseAsync");
    }

    [Fact]
    public void WorkflowStarter_SignalWithStartAsync_rethrows_on_failures()
    {
        AssertGeneralExceptionCatchRethrows("SignalWithStartAsync");
    }

    [Fact]
    public void DATA12_residuals_marked_on_named_surfaces()
    {
        var actorPath = FindSourceFile("TenantActivityPublisherActor.cs", "QimErp.Shared.Common");
        actorPath.Should().NotBeNull();
        var actorSource = File.ReadAllText(actorPath!);
        actorSource.Should().Contain(Data12MarkerPrefix,
            "TenantActivityPublisherActor must carry a DATA-12 trap comment");

        var starterPath = FindSourceFile("ApprovalWorkflowStarter.cs", "QimErp.Shared.Common");
        starterPath.Should().NotBeNull();
        var starterSource = File.ReadAllText(starterPath!);
        starterSource.Should().Contain(Data12MarkerPrefix,
            "ApprovalWorkflowStarter must carry a DATA-12 trap comment");
        starterSource.Should().Contain(
            "structured result for outbox-relay retry — not fail-closed rethrow",
            "ApprovalWorkflowStarter DATA-12 marker must name the ErrorMessage residual");
    }

    private static void AssertGeneralExceptionCatchRethrows(string methodName)
    {
        var path = FindSourceFile("WorkflowStarter.cs", "QFace.Sdk.Temporal");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        var methodBody = ExtractMethodBody(content, methodName);
        methodBody.Should().NotBeNullOrEmpty();

        var catchIdx = methodBody.IndexOf("catch (Exception", StringComparison.Ordinal);
        catchIdx.Should().BeGreaterThanOrEqualTo(0, $"{methodName} must have a catch (Exception) block");

        var catchBlock = ExtractBraceBlock(methodBody, catchIdx);
        catchBlock.Should().Contain("throw");
        catchBlock.Should().NotContain("WorkflowAlreadyStartedException");
    }

    private static string ExtractMethodBody(string source, string methodName)
    {
        var signatureIdx = source.IndexOf(methodName + "<", StringComparison.Ordinal);
        if (signatureIdx < 0)
            signatureIdx = source.IndexOf(methodName + "(", StringComparison.Ordinal);
        if (signatureIdx < 0)
            return string.Empty;

        var braceStart = source.IndexOf('{', signatureIdx);
        if (braceStart < 0)
            return string.Empty;

        return ExtractBraceBlock(source, braceStart);
    }

    private static string ExtractBraceBlock(string source, int fromIndex)
    {
        var braceStart = source.IndexOf('{', fromIndex);
        if (braceStart < 0)
            return string.Empty;

        var depth = 0;
        for (var i = braceStart; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return source[braceStart..(i + 1)];
            }
        }

        return string.Empty;
    }

    private static string? FindSourceFile(string fileName, string projectFolder)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var project = dir.GetDirectories(projectFolder, SearchOption.AllDirectories)
                .FirstOrDefault(d => !d.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                  && !d.Name.EndsWith("Tests", StringComparison.Ordinal));
            if (project is not null)
            {
                var match = project.GetFiles(fileName, SearchOption.AllDirectories).FirstOrDefault();
                if (match is not null) return match.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
