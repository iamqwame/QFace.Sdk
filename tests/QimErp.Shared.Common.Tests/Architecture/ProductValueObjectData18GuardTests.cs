using FluentAssertions;
using Xunit;

namespace QimErp.Shared.Common.Tests.Architecture;

/// <summary>
/// DATA-18: ProductValueObject persists ImageFileId via write-stop, not ImageUrl assignment.
/// </summary>
public class ProductValueObjectData18GuardTests
{
    [Fact]
    public void ProductValueObject_exposes_ImageFileId_write_stop_pattern()
    {
        var path = FindSourceFile("ProductValueObject.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("DATA-18");
        content.Should().Contain("ImageFileId");
        content.Should().Contain("WithImageFileId");
        content.Should().Contain("WithImageUrl");
        content.Should().NotContain("ImageUrl = imageUrl");
    }

    [Fact]
    public void ProductValueObject_EF_extension_maps_ImageFileId_column()
    {
        var path = FindSourceFile("EntityTypeBuilderProductValueObjectExtensions.cs", "QimErp.Shared.Common");
        path.Should().NotBeNull();
        var content = File.ReadAllText(path!);

        content.Should().Contain("p.ImageFileId");
        content.Should().Contain("{columnNamePrefix}ImageFileId");
        content.Should().Contain("all 11");
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
