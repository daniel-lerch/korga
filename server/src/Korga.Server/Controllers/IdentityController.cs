using Korga.ChurchTools;
using Korga.Server.ChurchTools.Hosting;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Korga.Server.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
    private readonly ChurchToolsApiFactory churchToolsApiFactory;

    public IdentityController(ChurchToolsApiFactory churchToolsApiFactory)
    {
        this.churchToolsApiFactory = churchToolsApiFactory;
    }

    [HttpPost("~/api/login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            using ChurchToolsApi churchTools = await churchToolsApiFactory.Login(request.Username, request.Password);
        }
        catch (HttpRequestException)
        {
            return BadRequest();
        }

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, ""),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Array.Empty<byte>()), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "",
            audience: "",
            claims,
            notBefore: null,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials);

        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
