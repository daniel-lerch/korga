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

public class LdapCommandTests
{
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

		ServiceProvider serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
		using IServiceScope serviceScope = serviceProvider.CreateScope();
		IOptions<LdapOptions> options = serviceScope.ServiceProvider.GetRequiredService<IOptions<LdapOptions>>();
		LdapService ldap = serviceScope.ServiceProvider.GetRequiredService<LdapService>();
		DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

		Assert.NotNull(ldap.GetMember(uid));

		DateTime earliest = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours).AddSeconds(-30);
		DateTime latest = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours).AddSeconds(30);
		
		Assert.True(await database.PasswordResets.AnyAsync(r => r.Uid == uid && r.Expiry > earliest && r.Expiry < latest));

		ldap.DeletePerson(uid);
	}

	[Fact]
	public async Task TestPasswd()
	{
		string uid = TestHost.RandomUid();

		ServiceProvider serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
		using IServiceScope serviceScope = serviceProvider.CreateScope();
		IOptions<LdapOptions> options = serviceScope.ServiceProvider.GetRequiredService<IOptions<LdapOptions>>();
		LdapService ldap = serviceScope.ServiceProvider.GetRequiredService<LdapService>();
		DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

		ldap.AddPerson(uid, "Max", "Mustermann", "mustermann@example.org");

		string[] args = new[] { "ldap", "passwd", uid };

		int returnCode = await TestHost.CreateCliHostBuilder().RunCommandLineApplicationAsync<KorgaCommand>(args, app =>
		{
			// This method disposes the host after shutdown. Therefore, it might be dangerous to dispose the scope after that.
			var scope = app.CreateScope();
			app.Conventions.UseConstructorInjection(scope.ServiceProvider);
		});

		Assert.Equal(0, returnCode);

		DateTime earliest = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours).AddSeconds(-30);
		DateTime latest = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours).AddSeconds(30);

		Assert.True(await database.PasswordResets.AnyAsync(r => r.Uid == uid && r.Expiry > earliest && r.Expiry < latest));

		ldap.DeletePerson(uid);
	}
}
