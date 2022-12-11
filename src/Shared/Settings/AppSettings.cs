namespace Shared.Settings
{
    public class AppSettings
    {
        public string BaseAddress { get; set; } = string.Empty;
        public JwtConfig JwtConfig { get; set; } = new JwtConfig();
        public SmtpConfig SmtpConfig { get; set; } = new SmtpConfig();
    }

    public class JwtConfig
    {
        public string Secret { get; set; } = string.Empty;
    }

    public class SmtpConfig
    {
        public bool Enabled { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
    }
}