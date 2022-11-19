﻿using Korga.Server.Configuration;
using Korga.Server.Ldap.ObjectClasses;
using Korga.Server.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands;

[Command("ldap")]
[Subcommand(typeof(Create))]
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

    [Command("delete")]
    public class Delete
    {
        [Argument(0)]
        public string? Uid { get; set; }

        private int OnExecute(IOptions<LdapOptions> options, LdapService ldap)
        {
            if (string.IsNullOrEmpty(Uid)) return 1;

            ldap.Delete($"uid={Uid},{options.Value.BaseDn}");
            return 0;
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
