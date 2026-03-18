using System.Text.Json.Serialization;

namespace Portal.Features.Auth.Domain.Responses
{
    public class LoginResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expire_in_minutes")]
        public string ExpireInMinutes { get; set; } = string.Empty;

    }

    public class LoginUsuarioResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("parceiro_id")]
        public string ParceiroId { get; set; } = string.Empty;
    }

    public class LoginPerfilResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;
    }
}
