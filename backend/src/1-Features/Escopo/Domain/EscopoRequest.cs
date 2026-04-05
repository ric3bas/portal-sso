using Portal.Domain.Base;
using Portal.Features.Escopo.Infra;

namespace Portal.Features.Escopo.Domain
{
    public class EscopoRequest : BaseRequest
    {
        public string Nome { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Validations.EscopoRequestValidator();
            return Validate(this, validator);
        }

        public EscopoCommand ToCommand()
        {
            return new EscopoCommand
            {
                Nome = this.Nome.Trim()
            };
        }
    }
}
