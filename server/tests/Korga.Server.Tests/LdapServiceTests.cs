﻿using Korga.Server.Configuration;
using Korga.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.DirectoryServices.Protocols;

namespace Korga.Server.Tests
{
    [TestClass]
    public class LdapServiceTests
    {
        // This variable is set by the test host
        private IServiceProvider serviceProvider = null!;

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
            try
            {
                ldapService.AddPerson($"uid=maxmust,{options.Value.BaseDn}", "Max", "Mustermann");
                Assert.AreEqual(count + 1, ldapService.GetMembers());
            }
            catch (DirectoryOperationException ex) when (ex.Response.ResultCode == ResultCode.EntryAlreadyExists)
            {
                Assert.IsTrue(count > 0);
            }
        }
    }
}
