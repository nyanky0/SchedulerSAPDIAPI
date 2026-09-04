namespace SOLTIUS_Web_API_Add_On.Authentication
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string SigningKey { get; set; } = "";
        public int AccessTokenMinutes { get; set; } = 60;
        public int RefreshTokenDays { get; set; } = 7;
        public List<OAuthClient> Clients { get; set; } = new();
    }

    public class OAuthClient
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string DisplayName { get; set; } = "";
    }
}
