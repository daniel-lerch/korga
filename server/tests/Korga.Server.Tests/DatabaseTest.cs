using Korga.Server.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Tests
{
    public abstract class DatabaseTest
    {
        // These variables are set by the test host
        protected IServiceProvider serviceProvider = null!;
        protected IServiceScope scope = null!;
        protected DatabaseContext database = null!;

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public virtual void Initialize()
        {
            serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
            scope = serviceProvider.CreateScope();
            database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        }

        [TestCleanup]
        public virtual async Task Cleanup()
        {
            scope.Dispose();

            var cleanupScope = serviceProvider.CreateScope();
            var database = cleanupScope.ServiceProvider.GetRequiredService<DatabaseContext>();

            database.RemoveRange(await database.People
                .Where(p => EF.Functions.Like(p.MailAddress, $"{TestContext.TestName.ToLowerInvariant()}%@unittest.example.com"))
                .ToArrayAsync());
            await database.SaveChangesAsync();

            cleanupScope.Dispose();
        }

        protected string GenerateMailAddress()
        {
            return $"{TestContext.TestName.ToLowerInvariant()}@unittest.example.com";
        }
    }
}
