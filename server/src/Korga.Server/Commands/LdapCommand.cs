using Korga.Ldap.Entities;
using Korga.Server.Configuration;
using Korga.Server.Ldap.ObjectClasses;
using Korga.Server.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands;

[Command("ldap")]
[Subcommand(typeof(Create))]
[Subcommand(typeof(Edit))]
[Subcommand(typeof(Passwd))]
[Subcommand(typeof(Delete))]
[Subcommand(typeof(List))]
public class LdapCommand
{
	private int OnExecute(CommandLineApplication app)
	{
		app.ShowHint();
		return 1;
	}

	[Command("create")]
	public class Create
	{
		[Option(Description = "Creates an organizational unit which can have members")]
		public bool OrganizationalUnit { get; set; }

		[Option(Description = "Override generated default UID with a custom value")]
		public string? Uid { get; set; }

		[Argument(0)] public string? GivenName { get; set; }
		[Argument(1)] public string? FamilyName { get; set; }
		[Argument(2)] public string? MailAddress { get; set; }

		private async Task<int> OnExecute(IConsole console, IOptions<LdapOptions> options, LdapService ldap, LdapUidService ldapUid, DatabaseContext database)
		{
			if (OrganizationalUnit)
			{
				ldap.AddOrganizationalUnit(options.Value.BaseDn, "members");
				return 0;
			}
			else
			{
				if (string.IsNullOrWhiteSpace(GivenName))
				{
					console.Out.WriteLine("Invalid given name");
					return 1;
				}

				if (string.IsNullOrWhiteSpace(FamilyName))
				{
					console.Out.WriteLine("Invalid family name");
					return 1;
				}

				if (MailAddress == null || !MailAddress.Contains('@'))
				{
					console.Out.WriteLine("Invalid email address");
					return 1;
				}

				if (string.IsNullOrWhiteSpace(Uid))
					Uid = ldapUid.GetUid(GivenName, FamilyName);

				ldap.AddPerson(Uid, GivenName, FamilyName, MailAddress);

				PasswordReset passwordReset = new(Uid)
				{
					Token = Guid.NewGuid(),
					Expiry = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours)
				};
				database.PasswordResets.Add(passwordReset);
				await database.SaveChangesAsync();

				console.Out.WriteLine("Password reset token: {0}", passwordReset.Token);
				return 0;
			}
		}
	}

	[Command("edit")]
	public class Edit
	{
		[Argument(0)]
		public string? Uid { get; set; }

		private int OnExecute(IConsole console, LdapService ldap)
		{
			if (string.IsNullOrEmpty(Uid)) return 1;

			InetOrgPerson? person = ldap.GetMember(Uid);
			if (person == null) return 2;

			console.Out.Write("Given name [{0}]: ", person.GivenName);
			string? givenName = console.In.ReadLine();
			if (givenName == null) return 1;
			if (givenName.Length > 0) person.GivenName = givenName;

			console.Out.Write("Family name [{0}]: ", person.Sn);
			string? familyName = console.In.ReadLine();
			if (familyName == null) return 1;
			if (familyName.Length > 0) person.Sn = familyName;

			console.Out.Write("Email address [{0}]: ", person.Mail);
			string? mailAddress = console.In.ReadLine();
			if (mailAddress == null) return 1;
			if (mailAddress.Length > 0) person.Mail = mailAddress;

			person.Cn = person.DisplayName = $"{person.GivenName} {person.Sn}";

			ldap.SavePerson(Uid, person);
			return 0;
		}
	}

	[Command("passwd")]
	public class Passwd
	{
		[Argument(0)]
		public string? Uid { get; set; }

		private async Task<int> OnExecuteAsync(IConsole console, IOptions<LdapOptions> options, LdapService ldap, DatabaseContext database)
		{
			if (string.IsNullOrEmpty(Uid)) return 1;

			InetOrgPerson? person = ldap.GetMember(Uid);
			if (person == null) return 2;

			PasswordReset passwordReset = new(Uid)
			{
				Token = Guid.NewGuid(),
				Expiry = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours)
			};
			database.PasswordResets.Add(passwordReset);
			await database.SaveChangesAsync();

			console.Out.WriteLine("Password reset token: {0}", passwordReset.Token);
			return 0;
		}
	}

	[Command("delete")]
	public class Delete
	{
		[Option(Description = "Deletes the main organizational unit which may have members")]
		public bool OrganizationalUnit { get; set; }

		[Argument(0)]
		public string? Uid { get; set; }

		private int OnExecute(IOptions<LdapOptions> options, LdapService ldap)
		{
			if (OrganizationalUnit)
			{
				if (Prompt.GetYesNo("Do you really want to delete the main organizational unit?", false))
				{
					ldap.Delete(options.Value.BaseDn);
					return 0;
				}
				else
				{
					return 1;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(Uid)) return 1;

				ldap.DeletePerson(Uid);
				return 0;
			}
		}
	}

	[Command("list")]
	public class List
	{
		private void OnExecute(IConsole console, LdapService ldap)
		{
			InetOrgPerson[] people = ldap.GetMembers();
			foreach (InetOrgPerson person in people)
			{
				console.Out.WriteLine($"uid={person.Uid} ({person.Cn})");
				console.Out.WriteLine("\tgivenName: {0}", person.GivenName);
				console.Out.WriteLine("\tsn: {0}", person.Sn);
				console.Out.WriteLine("\tdisplayName: {0}", person.DisplayName);
				console.Out.WriteLine("\tmail: {0}", person.Mail);
				console.Out.WriteLine("\tuserPassword: {0}", person.UserPassword);
			}
		}
	}
}
