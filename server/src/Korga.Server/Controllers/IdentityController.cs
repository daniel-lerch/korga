using Korga.Server.ChurchTools;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Korga.Server.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
    private readonly IOptions<ChurchToolsOptions> options;

    public IdentityController(IOptions<ChurchToolsOptions> options)
    {
        this.options = options;
    }

    [HttpPost("~/api/login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        using HttpClient httpClient = new() { BaseAddress = new UriBuilder("https", options.Value.Host).Uri };

        var response = await httpClient.PostAsJsonAsync("/api/login", new { request.Username, request.Password, RememberMe = false });

        return StatusCode(response.IsSuccessStatusCode ? 204 : 400);
    }
}
