using System.Security.Claims;

namespace Shared.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateJwtToken(Claim[] claims);
        Task<string> GenerateRefreshToken();
        Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token);
    }
}