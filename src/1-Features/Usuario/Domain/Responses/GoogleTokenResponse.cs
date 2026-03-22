using System.Text.Json.Serialization;

namespace Portal.Features.Usuario.Domain.Responses
{
    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? Access_Token { get; set; }

        [JsonPropertyName("id_token")]
        public string? Id_Token { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? Refresh_Token { get; set; }

        [JsonPropertyName("expires_in")]
        public int Expires_In { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
