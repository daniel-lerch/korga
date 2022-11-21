using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Ldap.ObjectClasses;
using Korga.Server.Models.Json;
using Korga.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Korga.Server.Controllers;

[ApiController]
public class PasswordResetController : ControllerBase
{
    private readonly DatabaseContext database;
    private readonly LdapService ldap;
    private readonly ILogger<PasswordResetController> logger;

    public PasswordResetController(DatabaseContext database, LdapService ldap, ILogger<PasswordResetController> logger)
    {
        this.database = database;
        this.ldap = ldap;
        this.logger = logger;
    }

    [HttpPost("~/api/password/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status410Gone)]
    [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
    {
        PasswordReset? passwordReset = await database.PasswordResets.SingleOrDefaultAsync(r => r.Token == request.Token);
        if (passwordReset == null) return StatusCode(StatusCodes.Status410Gone);

        if (passwordReset.Expiry < DateTime.UtcNow)
        {
            database.PasswordResets.Remove(passwordReset);
            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status410Gone);
        }

        InetOrgPerson? person = ldap.GetMember(passwordReset.Uid);
        if (person == null) return StatusCode(StatusCodes.Status500InternalServerError);

        person.UserPassword = request.PasswordHash;
        ldap.SavePerson(passwordReset.Uid, person);

        database.PasswordResets.Remove(passwordReset);
        await database.SaveChangesAsync();

        logger.LogInformation("Successfully changed LDAP password for {Uid}", passwordReset.Uid);

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
