using Korga.Models.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Korga.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    [HttpGet("~/api/profile")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public IActionResult Profile()
    {
        string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? displayName = User.FindFirstValue(ClaimTypes.Name);
        string? givenName = User.FindFirstValue(ClaimTypes.GivenName);
        string? familyName = User.FindFirstValue(ClaimTypes.Surname);
        string? emailAddress = User.FindFirstValue(ClaimTypes.Email);
        string? picture = User.FindFirstValue("picture");

        if (id == null || displayName == null || givenName == null || familyName == null || emailAddress == null) return new JsonResult(null);

        return new JsonResult(new ProfileResponse
        {
            Id = id,
            DisplayName = displayName,
            GivenName = givenName,
            FamilyName = familyName,
            EmailAddress = emailAddress,
            Picture = picture
        });
    }

    [HttpGet("~/api/challenge")]
    public IActionResult ChallengeLogin([FromQuery] string redirect)
    {
        AuthenticationProperties properties = new() { RedirectUri = redirect };
        return Challenge(properties, "OAuth");
    }

    [HttpGet("~/api/logout")]
    public IActionResult Logout()
    {
        return SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
