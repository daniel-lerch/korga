using Korga.Server.Services;
using Microsoft.Extensions.DependencyInjection;
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
            serviceProvider = TestServiceCollection.CreateDefault().BuildServiceProvider();
        }

        [TestMethod]
        public void TestGetMembers()
        {
            var ldapService = serviceProvider.GetRequiredService<LdapService>();
            Assert.AreEqual(0, ldapService.GetMembers());
        }
    }
}
