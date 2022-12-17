namespace Shared.Settings
{
    public class AppSettings
    {
        public string BaseAddress { get; set; } = string.Empty;
        public JwtConfig JwtConfig { get; set; } = null!;
        public SmtpConfig SmtpConfig { get; set; } = null!;
    }
}