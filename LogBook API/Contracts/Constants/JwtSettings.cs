namespace LogBook_API.Contracts.Constants
{
    public class JwtSettings
    {
        public string validIssuer { get; set; }
        public string validAudience { get; set; }
        public string secretKey { get; set; }
        public int expires { get; set; }
    }
}
