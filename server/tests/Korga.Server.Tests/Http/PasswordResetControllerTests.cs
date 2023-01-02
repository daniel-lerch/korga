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

namespace Korga.Server.Tests.Http;

public class PasswordResetControllerTests
{
	// This fixture instance will be used for all tests in this class: https://xunit.net/docs/shared-context
	public class Fixture : IDisposable
    {
        public Fixture()
        {
            Server = TestHost.CreateTestServer();
            Client = Server.CreateClient();
            ServiceScope = Server.Services.CreateScope();
            Database = ServiceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
            Ldap = ServiceScope.ServiceProvider.GetRequiredService<LdapService>();
        }

        public TestServer Server { get; }
        public HttpClient Client { get; }
        public IServiceScope ServiceScope { get; }
        public DatabaseContext Database { get; }
        public LdapService Ldap { get; }

        public void Dispose()
        {
            ServiceScope.Dispose();
            Client.Dispose();
            Server.Dispose();
        }
    }

    public class InvalidToken : IClassFixture<Fixture>
    {
        private readonly Fixture fixture;

        public InvalidToken(Fixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestPasswordResetInfo()
        {
            HttpResponseMessage response = await fixture.Client.GetAsync($"/api/password/reset?token={Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TestResetPassword()
        {
            HttpResponseMessage response =
                await fixture.Client.PostAsJsonAsync("/api/password/reset", new PasswordResetRequest(Guid.NewGuid(), "{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    public class ExpiredToken : IClassFixture<Fixture>, IDisposable
    {
        private readonly Fixture fixture;
        private readonly PasswordReset passwordReset;

        public ExpiredToken(Fixture fixture)
        {
            this.fixture = fixture;

            string uid = TestHost.RandomUid();
            fixture.Ldap.AddPerson(uid, "Max", "Mustermann", "max.mustermann@example.org");
            passwordReset = new PasswordReset(uid) { Token = Guid.NewGuid(), Expiry = DateTime.UtcNow.AddSeconds(-1) };
            fixture.Database.PasswordResets.Add(passwordReset);
            fixture.Database.SaveChanges();
        }

        public void Dispose()
        {
            fixture.Ldap.DeletePerson(passwordReset.Uid);
            PasswordReset? expired = fixture.Database.PasswordResets.SingleOrDefault(r => r.Token == passwordReset.Token);
            if (expired != null)
                fixture.Database.PasswordResets.Remove(expired);
            fixture.Database.SaveChanges();
        }

        [Fact]
        public async Task TestPasswordResetInfo()
        {
            HttpResponseMessage response = await fixture.Client.GetAsync($"/api/password/reset?token={passwordReset.Token}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            InetOrgPerson? person = fixture.Ldap.GetMember(passwordReset.Uid);
            Assert.NotNull(person);
            Assert.Null(person.UserPassword);
        }

        [Fact]
        public async Task TestResetPassword()
        {
            HttpResponseMessage response =
                await fixture.Client.PostAsJsonAsync("/api/password/reset", new PasswordResetRequest(passwordReset.Token, "{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            InetOrgPerson? person = fixture.Ldap.GetMember(passwordReset.Uid);
            Assert.NotNull(person);
            Assert.Null(person.UserPassword);
        }
    }

    public class ValidToken : IClassFixture<Fixture>, IDisposable
    {
        private readonly Fixture fixture;
        private readonly PasswordReset passwordReset;

        public ValidToken(Fixture fixture)
        {
            this.fixture = fixture;

            string uid = TestHost.RandomUid();
            fixture.Ldap.AddPerson(uid, "Max", "Mustermann", "max.mustermann@example.org");
            passwordReset = new PasswordReset(uid) { Token = Guid.NewGuid(), Expiry = DateTime.UtcNow.AddDays(1) };
            fixture.Database.PasswordResets.Add(passwordReset);
            fixture.Database.SaveChanges();
        }

        public void Dispose()
        {
            fixture.Ldap.DeletePerson(passwordReset.Uid);
            PasswordReset? valid = fixture.Database.PasswordResets.SingleOrDefault(r => r.Token == passwordReset.Token);
            if (valid != null)
                fixture.Database.PasswordResets.Remove(valid);
            fixture.Database.SaveChanges();
        }

        [Fact]
        public async Task TestPasswordResetInfo()
        {
            HttpResponseMessage response = await fixture.Client.GetAsync($"/api/password/reset?token={passwordReset.Token}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            InetOrgPerson? person = fixture.Ldap.GetMember(passwordReset.Uid);
            Assert.NotNull(person);
            Assert.Null(person.UserPassword);

            PasswordResetInfo? body = await response.Content.ReadFromJsonAsync<PasswordResetInfo>();
            Assert.NotNull(body);
            Assert.Equal(person.Uid, body.Uid);
            Assert.Equal(person.GivenName, body.GivenName);
            Assert.Equal(person.Sn, body.FamilyName);
        }

        [Fact]
        public async Task TestResetPassword()
        {
            HttpResponseMessage response =
                await fixture.Client.PostAsJsonAsync("/api/password/reset", new PasswordResetRequest(passwordReset.Token, "{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P"));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            InetOrgPerson? person = fixture.Ldap.GetMember(passwordReset.Uid);
            Assert.NotNull(person);
            Assert.Equal("{SSHA}HX69QAloGNlC4QyuKNE+HDAW7/yBkb2P", person.UserPassword);

            Assert.False(await fixture.Database.PasswordResets.AnyAsync(r => r.Token == passwordReset.Token));
        }
    }
}
