using Portal.Domain.Base;
using Portal.Features.Perfil.Infra;

namespace Portal.Features.Perfil.Domain
{
    public class PerfilRequest : BaseRequest
    {
        public string Nome { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Validations.PerfilRequestValidator();
            return Validate(this, validator);
        }

        public PerfilCommand ToCommand()
        {
            return new PerfilCommand
            {
                Nome = this.Nome,
            };
        }
    }
}
