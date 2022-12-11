using System.Text.Json.Serialization;
using Shared.Utils;

namespace ApiAuth.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; } = false;
        public string Role { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        [JsonIgnore]
        public byte[] Salt { get; set; } = HashManager.GenerateSalt();
    }
}