using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NectarineAPI.Configurations;
using NectarineData.Models;

namespace NectarineAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IOptions<TokenOptions> _tokenOptions;

        public TokenService(IOptions<TokenOptions> tokenOptions)
        {
            _tokenOptions = tokenOptions;
        }

        public string GenerateTokenAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenOptions.Value.Secret));
            var issuer = _tokenOptions.Value.Issuer;
            var audience = _tokenOptions.Value.Audience;

            var claims = new List<Claim>
            {
                new (ClaimTypes.Email, user.Email),
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
                Issuer = issuer,
                Audience = audience,
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}