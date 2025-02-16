using Korga.Filters;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Korga.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    private readonly PersonFilterService filterService;

    public ProfileController(PersonFilterService filterService)
    {
        this.filterService = filterService;
    }

    [HttpGet("~/api/profile")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Profile()
    {
        string? id = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        string? givenName = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
        string? familyName = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
        string? emailAddress = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        if (id == null || givenName == null || familyName == null || emailAddress == null) return new JsonResult(null);

        ProfileResponse response = new()
        {
            Id = id,
            GivenName = givenName,
            FamilyName = familyName,
            EmailAddress = emailAddress,
        };

        foreach (Permissions permission in Enum.GetValues<Permissions>())
        {
            response.Permissions[permission] = await filterService.HasPermission(User, permission);
        }

        return new JsonResult(response);
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
