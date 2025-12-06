using ChurchTools;
using Mailist.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mailist.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IOptions<JwtOptions> jwtOptions;

    public ProfileController(IOptions<JwtOptions> jwtOptions)
    {
        this.jwtOptions = jwtOptions;
    }

    [HttpPost("~/api/token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> GetAccessToken([FromBody] TokenRequest request)
    {
        using var churchToolsFromRequest = ChurchToolsApi.CreateWithToken(new Uri(request.ChurchToolsUrl), request.LoginToken);
        var user = await churchToolsFromRequest.GetPerson();
        var permissions = await churchToolsFromRequest.GetGlobalPermissions();

        if (permissions.Mailist == null)
            return BadRequest("User does not have permissions for Mailist or the Mailist plugin is not installed in ChurchTools");

        var module = await churchToolsFromRequest.GetCustomModule("mailist");
        var categories = await churchToolsFromRequest.GetCustomDataCategories(module.Id);
        var configCategory = categories.FirstOrDefault(c => c.Shorty == "config");
        if (configCategory == null)
            return BadRequest("Extension is not initialized. Custom data category 'config' was not found");

        bool isAdmin = permissions.Mailist.EditCustomData.Contains(configCategory.Id);

        var key = new SymmetricSecurityKey(Convert.FromHexString(jwtOptions.Value.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddHours(1);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // iat as Unix time (seconds)
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (isAdmin)
        {
            // Add a "role" claim so ASP.NET Core authorization recognizes it as a role.
            claims.Add(new Claim(ClaimTypes.Role, "admin"));
        }

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Value.Issuer,
            audience: jwtOptions.Value.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse { AccessToken = tokenString };
    }

    public class TokenRequest
    {
        public required string ChurchToolsUrl { get; init; }
        public required string LoginToken { get; init; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
