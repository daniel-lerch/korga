using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Tests.Database
{
    [TestClass]
    public class DatabaseHelperTests : DatabaseTest
    {
        [TestMethod]
        public async Task TestUpdatePerson_Concurrent()
        {
            Person person = new("Max", "Mustermann") { MailAddress = GenerateMailAddress() };
            database.People.Add(person);
            await database.SaveChangesAsync();

            await Task.WhenAll(Enumerable.Range(0, 16).Select(i => updatePerson(person.Id, i)));

            async Task updatePerson(int personId, int index)
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                Person person = await database.People.AsTracking().SingleAsync(p => p.Id == personId);

                await database.UpdatePerson(person, p =>
                {
                    p.GivenName = $"M{index}x";
                    p.FamilyName = $"Musterm{index}nn";
                });
            }

            Assert.AreEqual(16, await database.PersonSnapshots.CountAsync(ps => ps.PersonId == person.Id));
        }

        [TestMethod]
        public async Task TestDeletePerson_Concurrent()
        {
            Person person = new("Max", "Mustermann") { MailAddress = GenerateMailAddress() };
            database.People.Add(person);
            await database.SaveChangesAsync();

            bool[] results = await Task.WhenAll(Enumerable.Range(0, 16).Select(i => deletePerson(person.Id, null)));

            async Task<bool> deletePerson(int personId, int? deletedById)
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                Person person = await database.People.SingleAsync(p => p.Id == personId);
                return await database.DeleteEntity(person, deletedById);
            }

            Assert.AreEqual(1, results.Count(x => x == true));
        }
    }
}
