namespace Shared.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateJwtToken(string username, string role, string email);
    }
}