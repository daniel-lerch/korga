using Korga.ChurchTools.Entities;
using Korga.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Filters;

public class PersonLookupService
{
    private readonly IOptions<OpenIdConnectOptions> openIdConnectOptions;
    private readonly ILogger<PersonLookupService> logger;
    private readonly DatabaseContext database;

    public PersonLookupService(IOptions<OpenIdConnectOptions> openIdConnectOptions, ILogger<PersonLookupService> logger, DatabaseContext database)
    {
        this.openIdConnectOptions = openIdConnectOptions;
        this.logger = logger;
        this.database = database;
    }

    public async ValueTask<Person?> GetPerson(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        string? id = user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (string.IsNullOrEmpty(id))
        {
            logger.LogWarning("No name identifier claim found in user. "
                + $"{nameof(PersonLookupService)}.{nameof(GetPerson)} should only be called if the user is authenticated");
            return null;
        }

        string prefix = openIdConnectOptions.Value.ChurchToolsPersonIdPrefix;

        if (!id.StartsWith(prefix))
        {
            logger.LogError("Name identifier claim does not start with the expected prefix: \"{}\". " +
                $"This is likely caused by a wrong configuration of " +
                $"{nameof(OpenIdConnectOptions)}.{nameof(OpenIdConnectOptions.ChurchToolsPersonIdPrefix)} " +
                $"which does not match your OpenID Connect provider's sub claim format.", prefix);
            return null;
        }

        if (!int.TryParse(id.AsSpan(prefix.Length), out int personId))
        {
            logger.LogError("Name identifier claim final part {} could not be parsed as an integer. " +
                "Your OpenID Connect provider is likely not compatible with Korga.", id[prefix.Length..]);
            return null;
        }

        Person? person = await database.People.SingleOrDefaultAsync(p => p.Id == personId, cancellationToken);
        if (person == null)
        {
            logger.LogWarning("Person with ID {} not found in database. " +
                "Synchronization between Korga and ChurchTools might be lagging behind " +
                "or Korga's ChurchTools user might not have sufficient permissions to access all users.", personId);
        }

        return person;
    }
}
