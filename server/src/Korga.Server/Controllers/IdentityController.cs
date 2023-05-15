﻿using Korga.Server.ChurchTools;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
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

        if (!response.IsSuccessStatusCode) return BadRequest();

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