using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Korga.Server.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    [Authorize]
    [HttpGet("~/api/profile")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    public IActionResult Profile()
    {
        return new JsonResult(new ProfileResponse
        {
            Id = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"),
            GivenName = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"),
            FamilyName = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"),
            EmailAddress = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
        });
    }
}
