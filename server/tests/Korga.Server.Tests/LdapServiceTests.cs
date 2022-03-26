using Korga.Server.Configuration;
using Korga.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using Xunit;

namespace Korga.Server.Tests
{
    public class LdapServiceTests
    {
        private readonly IServiceProvider serviceProvider;

        public LdapServiceTests()
        {
            serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
        }

        [Fact]
        public void TestAddMember()
        {
            var options = serviceProvider.GetRequiredService<IOptions<LdapOptions>>();
            var ldapService = serviceProvider.GetRequiredService<LdapService>();
            int count = ldapService.GetMembers();
            try
            {
                ldapService.AddPerson($"uid=maxmust,{options.Value.BaseDn}", "Max", "Mustermann");
                Assert.Equal(count + 1, ldapService.GetMembers());
            }
            catch (DirectoryOperationException ex) when (ex.Response.ResultCode == ResultCode.EntryAlreadyExists)
            {
                Assert.True(count > 0);
            }
        }
    }
}
