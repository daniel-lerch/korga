using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Korga.Server.ChurchTools;
using Korga.Server.Configuration;
using Korga.Server.Database.Entities;
using System.Linq;
using System;

namespace Korga.Server.Services;

public class DistributionListService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly ILogger<DistributionListService> logger;
    private readonly ChurchToolsApiService churchTools;

    private Dictionary<string, Group>? groupForAlias;

    public DistributionListService(IOptions<EmailRelayOptions> options, ILogger<DistributionListService> logger, ChurchToolsApiService churchTools)
    {
        this.options = options;
        this.logger = logger;
        this.churchTools = churchTools;
    }

    public async ValueTask<Group?> GetGroupForAlias(string alias, CancellationToken cancellationToken)
    {
        groupForAlias ??= await GetGroupIdsForAliases(cancellationToken);

        return groupForAlias.TryGetValue(alias, out Group? result) ? result : null;
    }

    public async ValueTask<EmailRecipient[]> GetRecipientsForGroup(Group group, long emailId, CancellationToken cancellationToken)
    {
        var groupMembers = await churchTools.GetGroupMembers(group.Id, cancellationToken);

        List<(string EmailAddress, string GivenName, string FamilyName)> recipients = new();

        foreach (GroupMember member in groupMembers)
        {
            var person = await churchTools.GetPerson(member.PersonId, cancellationToken);

            if (!string.IsNullOrEmpty(person.Email))
            {
                recipients.Add((person.Email, person.FirstName, person.LastName));
            }
        }

        if (recipients.Count > 0)
        {
            // Avoid duplicate emails for married couples with a shared email address
            return recipients
                .GroupBy(r => r.EmailAddress)
                .Select(grouping => new EmailRecipient(
                    emailAddress: grouping.Key,
                    fullName: string.Join(", ", grouping.Select(r => r.GivenName)) + ' ' + grouping.First().FamilyName)
                { EmailId = emailId })
                .ToArray();
        }
        else
        {
            logger.LogWarning("Group {GroupName} (#{GroupId}) does not have a single member with email address. Emails to this group will not be forwarded",
                group.Name, group.Id);
            return Array.Empty<EmailRecipient>();
        }
    }

    private async ValueTask<Dictionary<string, Group>> GetGroupIdsForAliases(CancellationToken cancellationToken)
    {
        Dictionary<string, Group> groupForAlias = new();
        var groups = await churchTools.GetGroups(cancellationToken);

        foreach (Group group in groups)
        {
            if (group.Information.TryGetValue(options.Value.ChurchToolsEmailAliasGroupField, out JsonElement emailAliasElement)
                && emailAliasElement.ValueKind == JsonValueKind.String)
            {
                string? emailAlias = emailAliasElement.GetString();
                if (string.IsNullOrEmpty(emailAlias)) continue;

                if (groupForAlias.TryAdd(emailAlias, group))
                {
                    logger.LogDebug("Group {GroupName} (#{GroupId}) has alias {EmailAlias}", group.Name, group.Id, emailAlias);
                }
                else
                {
                    int conflictId = groupForAlias[emailAlias].Id;
                    Group conflict = groups.Single(g => g.Id == conflictId);
                    logger.LogWarning("Group {GroupName} (#{GroupId}) has alias {EmailAlias}, the same alias like {ConflictName} (#{ConflictId})",
                        group.Name, group.Id, emailAlias, conflict.Name, conflict.Id);
                }
            }
        }

        return groupForAlias;
    }
}
