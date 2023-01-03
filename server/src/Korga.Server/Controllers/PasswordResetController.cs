using Korga.Ldap.Entities;
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

    [HttpGet("~/api/password/reset")]
    [ProducesResponseType(typeof(PasswordResetInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PasswordResetInfo([FromQuery] Guid token)
    {
        PasswordReset? passwordReset = await database.PasswordResets.SingleOrDefaultAsync(r => r.Token == token);
        if (passwordReset == null) return StatusCode(StatusCodes.Status404NotFound);

        if (passwordReset.Expiry < DateTime.UtcNow) return StatusCode(StatusCodes.Status404NotFound);

        InetOrgPerson? person = ldap.GetMember(passwordReset.Uid);
        if (person == null) return StatusCode(StatusCodes.Status404NotFound);

        return new JsonResult(new PasswordResetInfo(person.Uid ?? string.Empty, person.GivenName ?? string.Empty, person.Sn));
    }

    [HttpPost("~/api/password/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
    {
        if (!ModelState.IsValid) return StatusCode(400);

        PasswordReset? passwordReset = await database.PasswordResets.SingleOrDefaultAsync(r => r.Token == request.Token);
        if (passwordReset == null) return StatusCode(StatusCodes.Status404NotFound);

        if (passwordReset.Expiry < DateTime.UtcNow) return StatusCode(StatusCodes.Status404NotFound);

        InetOrgPerson? person = ldap.GetMember(passwordReset.Uid);
        if (person == null) return StatusCode(StatusCodes.Status404NotFound);

        person.UserPassword = request.PasswordHash;
        ldap.SavePerson(passwordReset.Uid, person);

        database.PasswordResets.Remove(passwordReset);
        await database.SaveChangesAsync();

        logger.LogInformation("Successfully changed LDAP password for {Uid}", passwordReset.Uid);

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
