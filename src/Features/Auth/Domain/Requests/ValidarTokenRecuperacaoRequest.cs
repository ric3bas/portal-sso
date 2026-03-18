using Portal.Dominio;
using Portal.Features.Auth.Domain.Validations;

namespace Portal.Features.Auth.Domain.Requests
{
    public class ValidarTokenRecuperacaoRequest : BaseRequest
    {
        public string Token { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new ValidarTokenRecuperacaoValidator();
            return Validate(this, validator);
        }
    }
}
