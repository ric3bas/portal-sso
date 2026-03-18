using Portal.Dominio;
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

        public override bool IsValid()
        {
            var validator = new Validations.ParceiroAtualizarRequestValidator();
            return Validate(this, validator);
        }
    }
}
