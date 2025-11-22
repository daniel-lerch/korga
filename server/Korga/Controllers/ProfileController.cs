using ChurchTools;
using Korga.Configuration;
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

namespace Korga.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IOptions<JwtOptions> jwtOptions;
    private readonly IChurchToolsApi churchTools;

    public ProfileController(IOptions<JwtOptions> jwtOptions, IChurchToolsApi churchTools)
    {
        this.jwtOptions = jwtOptions;
        this.churchTools = churchTools;
    }

    [HttpPost("~/api/token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> GetAccessToken([FromBody] TokenRequest request)
    {
        var systemPermissions = await churchTools.GetGlobalPermissions();
        if (systemPermissions.Korga == null)
            return StatusCode(500, "Korga's system user does not have permissions for Korga or the Korga plugin is not installed in ChurchTools");
        if (!systemPermissions.Korga.View || systemPermissions.Korga.ViewCustomCategory.Count == 0)
            return StatusCode(500, "Korga's system user does not have permissions for Korga");

        var module = await churchTools.GetCustomModule("korga");
        var categories = await churchTools.GetCustomDataCategories(module.Id);
        var configCategory = categories.FirstOrDefault(c => c.Shorty == "config");
        if (configCategory == null)
            return BadRequest("Extension is not initialized. Custom data category 'config' was not found");

        using var churchToolsFromRequest = ChurchToolsApi.CreateWithToken(new Uri(request.ChurchToolsUrl), request.LoginToken);
        var user = await churchToolsFromRequest.GetPerson();
        var permissions = await churchToolsFromRequest.GetGlobalPermissions();

        if (permissions.Korga == null)
            return BadRequest("User does not have permissions for Korga or the Korga plugin is not installed in ChurchTools");

        if (!permissions.Korga.ViewCustomData.Contains(configCategory.Id))
            return BadRequest($"User is not permitted to view custom data category {configCategory.Id} of Korga");

        bool isAdmin = permissions.Korga.EditCustomData.Contains(configCategory.Id);

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
