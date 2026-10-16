using FluentAssertions;
using QimErp.Shared.Common.Entities.Helpers;
using Xunit;

namespace QimErp.Shared.Common.Tests.Entities;

public class ContactPersonCreateVersion7Tests
{
    [Fact]
    public void Create_Mints_A_Version7_Id()
    {
        var first = ContactPerson.Create("Ada Lovelace");
        var second = ContactPerson.Create("Kwame Nkrumah");

        first.Id.Should().NotBe(Guid.Empty);
        first.Id.Version.Should().Be(7);
        second.Id.Should().NotBe(first.Id);
        second.Id.Version.Should().Be(7);
    }
}
