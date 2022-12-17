using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Interfaces;
using Shared.Settings;

namespace Shared.Services
{
    public class JwtService : IJwtService
    {
        private readonly IOptions<AppSettings> _appSettings;

        public JwtService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;

            if (_appSettings.Value.JwtConfig is null)
                throw new System.Exception("JWT configuration is missing.");
        }

        public Task<string> GenerateJwtToken(Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Value.JwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor();
            tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            tokenDescriptor.Subject = new ClaimsIdentity(claims);
            tokenDescriptor.Expires = GetTokenExpirationTime();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        public Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Task.FromResult(Convert.ToBase64String(randomNumber));
        }

        public Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Value.JwtConfig.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return Task.FromResult(principal);
        }

        private DateTime GetTokenExpirationTime()
        {
            DateTime expirationTime;

            switch (_appSettings.Value.JwtConfig.ExpirationType.ToLower())
            {
                case "seconds":
                    expirationTime = DateTime.UtcNow.AddSeconds(_appSettings.Value.JwtConfig.Expiration);
                    break;
                case "minutes":
                    expirationTime = DateTime.UtcNow.AddMinutes(_appSettings.Value.JwtConfig.Expiration);
                    break;
                case "hours":
                    expirationTime = DateTime.UtcNow.AddHours(_appSettings.Value.JwtConfig.Expiration);
                    break;
                case "days":
                    expirationTime = DateTime.UtcNow.AddDays(_appSettings.Value.JwtConfig.Expiration);
                    break;
                default:
                    expirationTime = DateTime.UtcNow.AddSeconds(_appSettings.Value.JwtConfig.Expiration);
                    break;
            }

            return expirationTime;
        }
    }
}