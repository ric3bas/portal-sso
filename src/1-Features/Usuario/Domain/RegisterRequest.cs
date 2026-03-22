using Portal.Domain.Base;

namespace Portal.Features.Usuario.Domain
{
    public class RegisterRequest : BaseRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public int PerfilId { get; set; }

        public override bool IsValid()
        {
            var validator = new Validations.RegisterRequestValidator();
            return Validate(this, validator);
        }
    }
}