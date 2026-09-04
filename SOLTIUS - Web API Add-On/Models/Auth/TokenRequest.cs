using System.Text.Json.Serialization;

namespace SOLTIUS_Web_API_Add_On.Models.Auth
{
    public class TokenRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; } = "client_credentials";

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = "";

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = "";
    }
}
