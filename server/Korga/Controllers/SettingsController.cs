using Korga.Configuration;
using Korga.Models.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Korga.Controllers;

[ApiController]
public class SettingsController : ControllerBase
{
    private readonly IOptions<OpenIdConnectOptions> options;

    public SettingsController(IOptions<OpenIdConnectOptions> options)
    {
        this.options = options;
    }

    [HttpGet("~/api/settings")]
    [ProducesResponseType(typeof(SettingsResponse), StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        if (options.Value.Authority == null || options.Value.ClientId == null || options.Value.RedirectUri == null)
            return StatusCode(StatusCodes.Status500InternalServerError);

        return new JsonResult(new SettingsResponse
        {
            OidcAuthority = options.Value.Authority,
            OidcClientId = options.Value.ClientId,
            OidcRedirectUri = options.Value.RedirectUri,
        });
    }
}
