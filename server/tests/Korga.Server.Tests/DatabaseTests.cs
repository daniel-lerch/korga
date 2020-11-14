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
            int personId;

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                Person[] old = await database.People.Where(p => EF.Functions.Like(p.MailAddress, "mustermann%@example.com")).ToArrayAsync();
                database.People.RemoveRange(old);
                await database.SaveChangesAsync();

                var person = new Person("Max", "Mustermann")
                {
                    MailAddress = "mustermann@example.com"
                };
                database.People.Add(person);
                await database.SaveChangesAsync();

                personId = person.Id;
            }

            await Task.WhenAll(Enumerable.Range(0, 16).Select(i => addSnapshot(i)));

            async Task addSnapshot(int index)
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                Person person = await database.People.AsTracking().SingleAsync(p => p.Id == personId);

                await database.UpdatePerson(person, p =>
                {
                    p.MailAddress = $"mustermann{index}@example.com";
                    p.Version++;
                });
            }
        }
    }
}
