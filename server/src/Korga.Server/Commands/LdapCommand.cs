﻿using Korga.Server.Configuration;
using Korga.Server.Ldap.ObjectClasses;
using Korga.Server.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands;

[Command("ldap")]
[Subcommand(typeof(Create))]
[Subcommand(typeof(Edit))]
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

        private int OnExecute(IOptions<LdapOptions> options, LdapService ldap)
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
                if (givenName == null || givenName.Length < 3) return 1;

                Console.Write("Family name: ");
                string? familyName = Console.ReadLine();
                if (familyName == null || familyName.Length < 4) return 1;

                Console.Write("Email address: ");
                string? mailAddress = Console.ReadLine();
                if (mailAddress == null || !mailAddress.Contains('@')) return 1;

                string normalizedGivenName = givenName.ToLowerInvariant()
                    .Replace("ä", "ae", StringComparison.Ordinal)
                    .Replace("ö", "oe", StringComparison.Ordinal)
                    .Replace("ü", "ue", StringComparison.Ordinal)
                    .Replace("ß", "ss", StringComparison.Ordinal);
                string normalizedFamilyName = familyName.ToLowerInvariant()
                    .Replace("ä", "ae", StringComparison.Ordinal)
                    .Replace("ö", "oe", StringComparison.Ordinal)
                    .Replace("ü", "ue", StringComparison.Ordinal)
                    .Replace("ß", "ss", StringComparison.Ordinal);

                string uid = normalizedGivenName[0..3] + normalizedFamilyName[0..4];

                ldap.AddPerson($"uid={uid},{options.Value.BaseDn}", givenName, familyName, mailAddress);
                return 0;
            }
        }
    }

    [Command("edit")]
    public class Edit
    {
        [Argument(0)]
        public string? Uid { get; set; }

        private int OnExecute(IOptions<LdapOptions> options, LdapService ldap)
        {
            if (string.IsNullOrEmpty(Uid)) return 1;

            InetOrgPerson[] people = ldap.GetMembers();
            InetOrgPerson? person = people.SingleOrDefault(p => p.Uid == Uid);

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

            ldap.SavePerson($"uid={Uid},{options.Value.BaseDn}", person);
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

                ldap.Delete($"uid={Uid},{options.Value.BaseDn}");
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
            }
        }
    }
}
