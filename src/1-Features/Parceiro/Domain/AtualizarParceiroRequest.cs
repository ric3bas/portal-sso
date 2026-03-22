using Portal.Domain.Base;
using Portal.Features.Parceiro.Infra;
using System.Text.Json.Serialization;

namespace Portal.Features.Parceiro.Domain
{
    public class AtualizarParceiroRequest : BaseRequest
    {
        [JsonIgnore]
        public string? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }

        public ParceiroCommand ToCommand(Guid idParceiro) {
            return new ParceiroCommand {
                Id = Guid.Parse(Id!),
                Nome = Nome,
                Descricao = Descricao,
                Ativo = Ativo
            };
        }

        public override bool IsValid()
        {
            var validator = new Validations.ParceiroAtualizarRequestValidator();
            return Validate(this, validator);
        }
    }
}
