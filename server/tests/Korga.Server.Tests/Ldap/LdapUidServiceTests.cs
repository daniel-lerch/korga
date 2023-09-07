using Korga.Server.Services;
using Xunit;

namespace Korga.Server.Tests.Ldap;

public class LdapUidServiceTests
{
    private readonly LdapUidService ldapUid;

    public LdapUidServiceTests()
    {
        ldapUid = new LdapUidService();
    }

    [Fact]
    public void StandardName()
    {
        Assert.Equal("maxmust", ldapUid.GetUid("Max", "Mustermann"));
    }

    [Fact]
    public void WhiteSpaceFamilyName()
    {
        Assert.Equal("fravonb", ldapUid.GetUid("Franz", "von Baden"));
    }

    [Fact]
    public void NonAsciiChars()
    {
        Assert.Equal("zoegrae", ldapUid.GetUid("Zoé", "Gräf"));
    }

    [Fact]
    public void ShorterThanTemplate()
    {
        Assert.Equal("mado", ldapUid.GetUid("Ma", "Do"));
    }

    [Fact]
    public void SpecialChars()
    {
        Assert.Equal("petborb", ldapUid.GetUid("Peter", "Bor-Buchholz"));
    }
}
