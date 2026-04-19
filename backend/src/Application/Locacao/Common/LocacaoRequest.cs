using Portal.Application.Locacao.Validators;
using Portal.Domain.Base;

namespace Portal.Application.Locacao.Common
{
    public class LocacaoRequest : BaseRequest
    {
        public string ClienteId { get; set; } = string.Empty;
        public string EquipamentoId { get; set; } = string.Empty;
        public DateTime DataRetirada { get; set; }
        public DateTime PrevisaoDevolucao { get; set; }
        public decimal ValorDiaria { get; set; }
        public string? Observacao { get; set; }

        public override bool IsValid()
        {
            return Validate(this, new LocacaoRequestValidator());
        }
    }
}
