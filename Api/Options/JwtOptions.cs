namespace Api.Options
{
    public class JwtOptions
    {
        public string TokenSecret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double TokenExpiryMinutes { get; set; }
        public double RefreshExpiryHours { get; set; }

        public JwtOptions()
        {
            TokenSecret = string.Empty;
            Issuer = string.Empty;
            Audience = string.Empty;
            TokenExpiryMinutes = 60;
            RefreshExpiryHours = 24;
        }
    }
}
