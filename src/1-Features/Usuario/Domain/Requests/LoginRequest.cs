using System.Text.Json.Serialization;
using Portal.Domain.Base;

namespace Portal.Features.Auth.Domain.Requests
{
    public class LoginRequest : BaseRequest
    {
        [JsonPropertyName("login")]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("senha")]
        public string Senha { get; set; } = string.Empty;


        public override bool IsValid() {
        var validator = new Usuario.Domain.Validations.LoginRequestValidator();
            return Validate(this, validator);
        }
    }
}