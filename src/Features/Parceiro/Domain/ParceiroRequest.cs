using Portal.Dominio;
using ParceiroEntity = Portal.Domain.Entities.ParceiroEntity;

namespace Portal.Features.Parceiro.Domain {
    public class ParceiroRequest : BaseRequest {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }

        public ParceiroEntity ToEntity(Guid tenantId) => new ParceiroEntity {
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