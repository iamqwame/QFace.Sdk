using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-12: SDK <c>WorkflowStarter</c> fail-closed rethrow on start failures
/// (WorkflowAlreadyStartedException excluded). Named residuals live in DECISIONS 2026-10-20.
/// </summary>
public class SdkData12GuardTests
{
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
    public void DATA12_residuals_documented_in_DECISIONS()
    {
        var path = FindDecisionsFile();
        path.Should().NotBeNull("monorepo docs/architecture/DECISIONS.md must be reachable from the test bin");
        var content = File.ReadAllText(path!);

        var heading = "## 2026-10-20 — SDK DATA-09 sdk_surface + DATA-12 observability residuals";
        var start = content.IndexOf(heading, StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0, "2026-10-20 SDK DATA-09/DATA-12 entry must exist");

        var next = content.IndexOf("\n## ", start + heading.Length, StringComparison.Ordinal);
        var section = next < 0 ? content[start..] : content[start..next];

        section.Should().Contain("TenantActivityPublisherActor");
        section.Should().Contain("ApprovalWorkflowStarter");
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

    private static string? FindDecisionsFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "architecture", "DECISIONS.md");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return null;
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
