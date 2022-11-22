using Korga.Server.Configuration;
using Korga.Server.Ldap.ObjectClasses;
using Korga.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace Korga.Server.Tests;

public class LdapServiceTests
{
    private readonly IServiceProvider serviceProvider;

    public LdapServiceTests()
    {
        serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
    }

    [Fact]
    public void TestAddAndDeleteMember()
    {
        var options = serviceProvider.GetRequiredService<IOptions<LdapOptions>>();
        var ldapService = serviceProvider.GetRequiredService<LdapService>();

        string uid = TestHost.RandomUid();

        ldapService.AddPerson(uid, "Max", "Mustermann", "max.mustermann@example.org");
        InetOrgPerson? queried = ldapService.GetMember(uid);
        Assert.NotNull(queried);
        Assert.Equal("Max", queried.GivenName);
        Assert.Equal("Mustermann", queried.Sn);
        Assert.Equal("max.mustermann@example.org", queried.Mail);

        ldapService.DeletePerson(uid);
        queried = ldapService.GetMember(uid);
        Assert.Null(queried);
    }
}
