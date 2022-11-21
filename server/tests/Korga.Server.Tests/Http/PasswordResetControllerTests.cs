using Korga.Server.Database.Entities;
using Korga.Server.Database;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Korga.Server.Services;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Korga.Server.Ldap.ObjectClasses;

namespace Korga.Server.Tests.Http
{
    public class PasswordResetControllerTests : IDisposable
    {
        private readonly TestServer server;
        private readonly HttpClient client;
        private readonly IServiceScope serviceScope;
        private readonly DatabaseContext database;
        private readonly LdapService ldap;

        private readonly PasswordReset testReset_Expired;
        private readonly PasswordReset testReset_Valid;

        public PasswordResetControllerTests()
        {
            server = TestHost.CreateTestServer();
            client = server.CreateClient();
            serviceScope = server.Services.CreateScope();
            database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
            ldap = serviceScope.ServiceProvider.GetRequiredService<LdapService>();

            string uid_Expired = TestHost.RandomUid();
            ldap.AddPerson(uid_Expired, "Max", "Mustermann", "max.mustermann@example.org");
            testReset_Expired = new PasswordReset(uid_Expired) { Token = Guid.NewGuid(), Expiry = DateTime.UtcNow.AddSeconds(-1) };
            database.PasswordResets.Add(testReset_Expired);

            string uid_Valid = TestHost.RandomUid();
            ldap.AddPerson(uid_Valid, "Max", "Mustermann", "max.mustermann@example.org");
            testReset_Valid = new PasswordReset(uid_Valid) { Token = Guid.NewGuid(), Expiry = DateTime.UtcNow.AddDays(1) };
            database.PasswordResets.Add(testReset_Valid);

            database.SaveChanges();
        }

        public void Dispose()
        {
            ldap.DeletePerson(testReset_Expired.Uid);
            PasswordReset? expired = database.PasswordResets.SingleOrDefault(r => r.Token == testReset_Expired.Token);
            if (expired != null)
                database.PasswordResets.Remove(expired);

            ldap.DeletePerson(testReset_Valid.Uid);
            PasswordReset? valid = database.PasswordResets.SingleOrDefault(r => r.Token == testReset_Valid.Token);
            if (valid != null)
                database.PasswordResets.Remove(valid);

            database.SaveChanges();
            serviceScope.Dispose();
            server.Dispose();
            client.Dispose();
        }

        [Fact]
        public async Task TestResetPassword_InvalidToken()
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync("/api/password/reset", new PasswordResetRequest(Guid.NewGuid(), "{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P"));

            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        }

        [Fact]
        public async Task TestResetPassword_Expired()
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync("/api/password/reset", new PasswordResetRequest(testReset_Expired.Token, "{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P"));

            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);

            InetOrgPerson? person = ldap.GetMember(testReset_Expired.Uid);
            Assert.NotNull(person);
            Assert.Null(person.UserPassword);

            Assert.False(await database.PasswordResets.AnyAsync(r => r.Token == testReset_Expired.Token));
        }

        [Fact]
        public async Task TestResetPassword_Valid()
        {
            HttpResponseMessage response =
                await client.PostAsJsonAsync("/api/password/reset", new PasswordResetRequest(testReset_Valid.Token, "{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            InetOrgPerson? person = ldap.GetMember(testReset_Valid.Uid);
            Assert.NotNull(person);
            Assert.Equal("{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P", person.UserPassword);

            Assert.False(await database.PasswordResets.AnyAsync(r => r.Token == testReset_Valid.Token));
        }
    }
}
