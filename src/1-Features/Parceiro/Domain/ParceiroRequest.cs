using Portal.Domain.Base;
using Portal.Features.Parceiro.Infra;

namespace Portal.Features.Parceiro.Domain {
    public class ParceiroRequest : BaseRequest {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }

        public ParceiroCommand ToCommand(Guid tenantId) => new ParceiroCommand
        {
            Id        = tenantId,
            Nome      = Nome,
            Descricao = Descricao
        };

        public override bool IsValid() {
            var validator = new Validations.ParceiroRequestValidator();
            return Validate(this, validator);
        }
    }
}