using Korga.Server.Commands;
using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests.Commands;

public class LdapCommandTests : IClassFixture<LdapCommandTests.Fixture>
{
	public class Fixture : IDisposable
	{
		public Fixture()
		{
			ServiceProvider serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
			ServiceScope = serviceProvider.CreateScope();
			LdapOptions = ServiceScope.ServiceProvider.GetRequiredService<IOptions<LdapOptions>>();
			Database = ServiceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
			Ldap = ServiceScope.ServiceProvider.GetRequiredService<LdapService>();
		}

		public IServiceScope ServiceScope { get; }
		public IOptions<LdapOptions> LdapOptions { get; }
		public DatabaseContext Database { get; }
		public LdapService Ldap { get; }

		public void Dispose()
		{
			ServiceScope.Dispose();
		}
	}

	public class ValidUser : IClassFixture<Fixture>
	{
		private readonly Fixture fixture;

		public ValidUser(Fixture fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public async Task TestCreateUser()
		{
			string uid = TestHost.RandomUid();

			string[] args = new[] { "ldap", "create", $"--uid={uid}", "Max", "Mustermann", "mustermann@example.org" };

			int returnCode = await TestHost.CreateCliHostBuilder().RunCommandLineApplicationAsync<KorgaCommand>(args, app =>
			{
				// This method disposes the host after shutdown. Therefore, it might be dangerous to dispose the scope after that.
				var scope = app.CreateScope();
				app.Conventions.UseConstructorInjection(scope.ServiceProvider);
			});

			Assert.Equal(0, returnCode);

			Assert.NotNull(fixture.Ldap.GetMember(uid));

			DateTime earliest = DateTime.UtcNow.AddHours(fixture.LdapOptions.Value.PasswordResetExpiryHours).AddSeconds(-30);
			DateTime latest = DateTime.UtcNow.AddHours(fixture.LdapOptions.Value.PasswordResetExpiryHours).AddSeconds(30);

			Assert.True(await fixture.Database.PasswordResets.AnyAsync(r => r.Uid == uid && r.Expiry > earliest && r.Expiry < latest));

			fixture.Ldap.DeletePerson(uid);
		}

		[Fact]
		public async Task TestPasswd()
		{
			string uid = TestHost.RandomUid();

			fixture.Ldap.AddPerson(uid, "Max", "Mustermann", "mustermann@example.org");

			string[] args = new[] { "ldap", "passwd", uid };

			int returnCode = await TestHost.CreateCliHostBuilder().RunCommandLineApplicationAsync<KorgaCommand>(args, app =>
			{
				// This method disposes the host after shutdown. Therefore, it might be dangerous to dispose the scope after that.
				var scope = app.CreateScope();
				app.Conventions.UseConstructorInjection(scope.ServiceProvider);
			});

			Assert.Equal(0, returnCode);

			DateTime earliest = DateTime.UtcNow.AddHours(fixture.LdapOptions.Value.PasswordResetExpiryHours).AddSeconds(-30);
			DateTime latest = DateTime.UtcNow.AddHours(fixture.LdapOptions.Value.PasswordResetExpiryHours).AddSeconds(30);

			Assert.True(await fixture.Database.PasswordResets.AnyAsync(r => r.Uid == uid && r.Expiry > earliest && r.Expiry < latest));

			fixture.Ldap.DeletePerson(uid);
		}
	}
}
