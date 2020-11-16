using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Tests
{
    [TestClass]
    public class DatabaseTests
    {
        // This variable is set by the test host
        private IServiceProvider serviceProvider = null!;

        [TestInitialize]
        public void Initialize()
        {
            serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
        }

        [TestMethod]
        public async Task TestConcurrentSnapshots()
        {
            await Task.WhenAll(Enumerable.Range(0, 16).Select(i => addSnapshot(i)));

            async Task addSnapshot(int index)
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                Person person = await database.People.AsTracking().SingleAsync(p => p.GivenName == "Max" && p.FamilyName == "Mustermann");

                await database.UpdatePerson(person, p =>
                {
                    p.MailAddress = $"mustermann{index}@example.com";
                    p.Version++;
                });
            }
        }
    }
}
