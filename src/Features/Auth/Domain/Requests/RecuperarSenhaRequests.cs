using Portal.Dominio;
using Portal.Features.Auth.Domain.Validations;

namespace Portal.Features.Auth.Domain.Requests
{
    public class RecuperarSenhaRequest : BaseRequest
    {
        public string Login { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new RecuperarSenhaValidator();
            return Validate(this, validator);
        }
    }
}
