using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.ValidarTokenRecuperacao
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
