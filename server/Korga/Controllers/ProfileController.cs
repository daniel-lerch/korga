using Korga.Models.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;

namespace Korga.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IOptions<Configuration.OpenIdConnectOptions> openIdConnectOptions;

    public ProfileController(IOptions<Configuration.OpenIdConnectOptions> openIdConnectOptions)
    {
        this.openIdConnectOptions = openIdConnectOptions;
    }

    [HttpGet("~/api/profile")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public IActionResult Profile()
    {
        string? id = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        string? givenName = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
        string? familyName = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
        string? emailAddress = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        if (id == null || givenName == null || familyName == null || emailAddress == null) return new JsonResult(null);

        string prefix = openIdConnectOptions.Value.ChurchToolsPersonIdPrefix;

        int? churchToolsPersonId = null;

        if (id.StartsWith(prefix) && int.TryParse(id.AsSpan(prefix.Length), out int personId))
            churchToolsPersonId = personId;

        return new JsonResult(new ProfileResponse
        {
            Id = id,
            GivenName = givenName,
            FamilyName = familyName,
            EmailAddress = emailAddress,
            ChurchToolsPersonId = churchToolsPersonId
        });
    }

    [Authorize]
    [HttpGet("~/api/challenge")]
    public NoContentResult ChallengeLogin()
    {
        return NoContent();
    }

    [HttpGet("~/api/logout")]
    public IActionResult Logout()
    {
        return new SignOutResult(
        [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        ]);
    }
}
