using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private IServiceProvider? serviceProvider;

        [TestInitialize]
        public void Initialize()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var services = new ServiceCollection();
            services.ConfigureKorga(configuration);
            services.AddDbContext<DatabaseContext>();
            serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task TestConcurrentSnapshots()
        {
            int personId;

            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var snapshot = new PersonSnapshot("Max", "Mustermann")
                {
                    Person = new Person
                    {
                        Version = 1,
                        MailAddress = "max.mustermann@example.com"
                    },
                    Version = 1,
                    MailAddress = "max.mustermann@example.com"
                };
                database.PersonSnapshots.Add(snapshot);
                await database.SaveChangesAsync();

                personId = snapshot.PersonId;
            }

            await Task.WhenAll(Enumerable.Range(0, 16).Select(i => addSnapshot()));

            async Task addSnapshot()
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                Person person = await database.People.AsTracking().SingleAsync(p => p.Id == personId);

                while (true)
                {
                    try
                    {
                        var snapshot = new PersonSnapshot("Max", "Mustermann")
                        {
                            PersonId = personId,
                            Version = ++person.Version
                        };

                        database.PersonSnapshots.Add(snapshot);

                        await database.SaveChangesAsync();

                        break;
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (ex.Entries.Count != 1 || !(ex.Entries[0].Entity is Person))
                            throw new NotSupportedException("Concurrency conflicts can only be handled for a single entity of type " + nameof(Person), ex);

                        var entry = ex.Entries[0];
                        var databaseValues = await entry.GetDatabaseValuesAsync();
                        entry.OriginalValues.SetValues(databaseValues);
                        entry.CurrentValues.SetValues(databaseValues);
                    }
                }
            }
        }
    }
}
