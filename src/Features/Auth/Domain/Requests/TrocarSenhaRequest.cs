using Portal.Dominio;

namespace Portal.Features.Auth.Domain.Requests
{
    public class TrocarSenhaRequest : BaseRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NovaSenha { get; set; } = string.Empty;
        public string ConfirmarSenha { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Validations.TrocarSenhaRequestValidator();
            return Validate(this, validator);
        }
    }
}
