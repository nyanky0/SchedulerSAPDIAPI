using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SOLTIUS_Web_API_Add_On.Authentication;
using SOLTIUS_Web_API_Add_On.Models.Auth;

namespace SOLTIUS_Web_API_Add_On.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("oauth2")]
    public class AuthController : ControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        private readonly RefreshTokenStore _refreshStore;

        public AuthController(IOptions<JwtOptions> jwtOptions, RefreshTokenStore refreshStore)
        {
            _jwtOptions = jwtOptions.Value;
            _refreshStore = refreshStore;
        }

        [HttpPost("token")]
        public IActionResult Token([FromBody] TokenRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "invalid_request" });

            if (!string.Equals(request.GrantType, "client_credentials", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "unsupported_grant_type" });

            OAuthClient client = _jwtOptions.Clients?.FirstOrDefault(x =>
                string.Equals(x.ClientId, request.ClientId, StringComparison.Ordinal) &&
                string.Equals(x.ClientSecret, request.ClientSecret, StringComparison.Ordinal));

            if (client == null)
                return Unauthorized(new { error = "invalid_client" });

            if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey) || _jwtOptions.SigningKey.Length < 32)
                return StatusCode(500, new { error = "jwt_signing_key_invalid" });

            string accessToken = GenerateAccessToken(client, out DateTime expiresAt);
            string refreshToken = _refreshStore.Create(client.ClientId);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                ExpiresIn = (int)Math.Round((expiresAt - DateTime.UtcNow).TotalSeconds),
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { error = "invalid_request" });

            string clientId = _refreshStore.ValidateAndConsume(request.RefreshToken);
            if (clientId == null)
                return Unauthorized(new { error = "invalid_refresh_token" });

            OAuthClient client = _jwtOptions.Clients?.FirstOrDefault(x =>
                string.Equals(x.ClientId, clientId, StringComparison.Ordinal));

            if (client == null)
                return Unauthorized(new { error = "invalid_client" });

            if (!string.Equals(client.ClientId, request.ClientId, StringComparison.Ordinal) ||
                !string.Equals(client.ClientSecret, request.ClientSecret, StringComparison.Ordinal))
                return Unauthorized(new { error = "invalid_client" });

            string accessToken = GenerateAccessToken(client, out DateTime expiresAt);
            string newRefreshToken = _refreshStore.Create(client.ClientId);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                ExpiresIn = (int)Math.Round((expiresAt - DateTime.UtcNow).TotalSeconds),
                RefreshToken = newRefreshToken
            });
        }

        private string GenerateAccessToken(OAuthClient client, out DateTime expiresAt)
        {
            expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _jwtOptions.AccessTokenMinutes));
            byte[] keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, client.ClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim("client_id", client.ClientId),
                new Claim("client_name", client.DisplayName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
