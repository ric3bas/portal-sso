using Portal.Domain.Base;
using Portal.Features.Locacao.Domain.Validations;

namespace Portal.Features.Locacao.Domain
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

    public class AtualizarLocacaoRequest : BaseRequest
    {
        public string? Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public string EquipamentoId { get; set; } = string.Empty;
        public DateTime DataRetirada { get; set; }
        public DateTime PrevisaoDevolucao { get; set; }
        public decimal ValorDiaria { get; set; }
        public string? Observacao { get; set; }

        public override bool IsValid()
        {
            return Validate(this, new AtualizarLocacaoRequestValidator());
        }
    }

    public class DevolverLocacaoRequest : BaseRequest
    {
        public string? Id { get; set; }
        public DateTime DataDevolucao { get; set; }
        public string? Observacao { get; set; }

        public override bool IsValid()
        {
            return Validate(this, new DevolverLocacaoRequestValidator());
        }
    }

    public class FiltroLocacaoRequest
    {
        public string? ClienteId { get; set; }
        public string? EquipamentoId { get; set; }
        public StatusLocacao? Status { get; set; }
        public DateTime? DataRetiradaInicio { get; set; }
        public DateTime? DataRetiradaFim { get; set; }
    }
}