using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-16: global enum→string convention; Import.Status sole int exception.
/// </summary>
public class SdkData16GuardTests
{
    [Fact]
    public void EnumToStringConvention_preserves_Import_Status_int_exception()
    {
        var path = FindSourceFile("EnumToStringConvention.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("DATA-16");
        content.Should().Contain("typeof(Import)");
        content.Should().Contain("nameof(Import.Status)");
        content.Should().Contain("EnumToNumberConverter<ImportStatus, int>");
    }

    [Fact]
    public void ImportConfiguration_persists_Status_as_int()
    {
        var path = FindSourceFile("ImportConfiguration.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("builder.Property(i => i.Status)");
        content.Should().Contain(".HasConversion<int>()");
    }

    [Fact]
    public void ApplicationDbContext_registers_enum_to_string_convention_and_ImportConfiguration()
    {
        var path = FindSourceFile("ApplicationDbContext.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("new EnumToStringConvention()");
        content.Should().Contain("Properties<Enum>().HaveConversion<string>()");
        content.Should().Contain("ApplyConfiguration(new ImportConfiguration())");
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
