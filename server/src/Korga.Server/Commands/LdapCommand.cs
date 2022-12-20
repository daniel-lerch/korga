using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Extensions;
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

		private async Task<int> OnExecute(IOptions<LdapOptions> options, LdapService ldap, LdapUidService ldapUid, DatabaseContext database)
		{
			if (OrganizationalUnit)
			{
				ldap.AddOrganizationalUnit(options.Value.BaseDn, "members");
				return 0;
			}
			else
			{
				Console.Write("Given name: ");
				string? givenName = Console.ReadLine();
				if (givenName == null || givenName.Length == 0)
				{
					Console.WriteLine("Invalid given name");
					return 1;
				}

				Console.Write("Family name: ");
				string? familyName = Console.ReadLine();
				if (familyName == null || familyName.Length == 0)
				{
					Console.WriteLine("Invalid family name");
					return 1;
				}

				Console.Write("Email address: ");
				string? mailAddress = Console.ReadLine();
				if (mailAddress == null || !mailAddress.Contains('@'))
				{
					Console.WriteLine("Invalid email address");
					return 1;
				}

				string uid = ldapUid.GetUid(givenName, familyName);

				ldap.AddPerson(uid, givenName, familyName, mailAddress);

				PasswordReset passwordReset = new(uid)
				{
					Token = Guid.NewGuid(),
					Expiry = DateTime.UtcNow.AddHours(options.Value.PasswordResetExpiryHours)
				};
				database.PasswordResets.Add(passwordReset);
				await database.SaveChangesAsync();

				Console.WriteLine("Password reset token: {0}", passwordReset.Token);
				return 0;
			}
		}
	}

	[Command("edit")]
	public class Edit
	{
		[Argument(0)]
		public string? Uid { get; set; }

		private int OnExecute(LdapService ldap)
		{
			if (string.IsNullOrEmpty(Uid)) return 1;

			InetOrgPerson? person = ldap.GetMember(Uid);
			if (person == null) return 2;

			Console.Write("Given name [{0}]: ", person.GivenName);
			string? givenName = Console.ReadLine();
			if (givenName == null) return 1;
			if (givenName.Length > 0) person.GivenName = givenName;

			Console.Write("Family name [{0}]: ", person.Sn);
			string? familyName = Console.ReadLine();
			if (familyName == null) return 1;
			if (familyName.Length > 0) person.Sn = familyName;

			Console.Write("Email address [{0}]: ", person.Mail);
			string? mailAddress = Console.ReadLine();
			if (mailAddress == null) return 1;
			if (mailAddress.Length > 0) person.Mail = mailAddress;

			person.Cn = person.DisplayName = $"{givenName} {familyName}";

			ldap.SavePerson(Uid, person);
			return 0;
		}
	}

	[Command("passwd")]
	public class Passwd
	{
		[Argument(0)]
		public string? Uid { get; set; }

		private async Task<int> OnExecuteAsync(LdapService ldap, DatabaseContext database)
		{
			if (string.IsNullOrEmpty(Uid)) return 1;

			InetOrgPerson? person = ldap.GetMember(Uid);
			if (person == null) return 2;

			PasswordReset passwordReset = new(Uid)
			{
				Token = Guid.NewGuid(),
				Expiry = DateTime.UtcNow.AddDays(1)
			};
			database.PasswordResets.Add(passwordReset);
			await database.SaveChangesAsync();

			Console.WriteLine("Password reset token: {0}", passwordReset.Token);
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
		private void OnExecute(LdapService ldap)
		{
			InetOrgPerson[] people = ldap.GetMembers();
			foreach (InetOrgPerson person in people)
			{
				Console.WriteLine($"uid={person.Uid} ({person.Cn})");
				Console.WriteLine("\tgivenName: {0}", person.GivenName);
				Console.WriteLine("\tsn: {0}", person.Sn);
				Console.WriteLine("\tdisplayName: {0}", person.DisplayName);
				Console.WriteLine("\tmail: {0}", person.Mail);
				Console.WriteLine("\tuserPassword: {0}", person.UserPassword);
			}
		}
	}
}
