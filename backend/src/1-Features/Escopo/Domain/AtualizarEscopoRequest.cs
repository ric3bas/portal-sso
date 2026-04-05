using Portal.Domain.Base;
using Portal.Features.Escopo.Infra;
using System.Text.Json.Serialization;

namespace Portal.Features.Escopo.Domain
{

    public class AtualizarEscopoRequest : BaseRequest
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Validations.EscopoAtualizarRequestValidator();
            return Validate(this, validator);
        }

        public EscopoCommand ToCommand()
        {
            return new EscopoCommand
            {
                Id = this.Id,
                Nome = this.Nome
            };
        }
    }
}
