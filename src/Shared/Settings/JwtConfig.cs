namespace Shared.Settings
{
    public class JwtConfig
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string ExpirationType { get; set; } = string.Empty;
        public int Expiration { get; set; }
    }
}