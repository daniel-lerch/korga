using Korga.Server.Configuration;
using Korga.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Korga.Server.Tests
{
    [TestClass]
    public class LdapServiceTests
    {
        private IServiceProvider? serviceProvider;

        [TestInitialize]
        public void Initizalize()
        {
            serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
        }

        [TestMethod]
        public void TestAddMember()
        {
            var options = serviceProvider.GetRequiredService<IOptions<LdapOptions>>();
            var ldapService = serviceProvider.GetRequiredService<LdapService>();
            int count = ldapService.GetMembers();
            ldapService.AddPerson($"uid=maxmust,{options.Value.BaseDn}", "Max", "Mustermann");
            Assert.AreEqual(count + 1, ldapService.GetMembers());
        }
    }
}
