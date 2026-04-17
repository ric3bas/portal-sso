using Portal.Domain.Base;
using Portal.Features.Equipamento.Domain.Validations;

namespace Portal.Features.Equipamento.Domain
{
    public class EquipamentoRequest : BaseRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string CategoriaId { get; set; } = string.Empty;
        public int QuantidadeEstoque { get; set; }
        public decimal PrecoDiaria { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int AnoFabricacao { get; set; }
        public string? Descricao { get; set; }
        public string? ObservacaoInternas { get; set; }

        public override bool IsValid()
        {
            var validator = new EquipamentoRequestValidator();
            var result = validator.Validate(this);
            return result.IsValid;
        }
    }

    public class AtualizarEquipamentoRequest : BaseRequest
    {
        public string? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CategoriaId { get; set; } = string.Empty;
        public int QuantidadeEstoque { get; set; }
        public decimal PrecoDiaria { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int AnoFabricacao { get; set; }
        public string? Descricao { get; set; }
        public string? ObservacaoInternas { get; set; }
        public bool? Ativo { get; set; }

        public override bool IsValid()
        {
            var validator = new AtualizarEquipamentoRequestValidator();
            var result = validator.Validate(this);
            return result.IsValid;
        }
    }

    public class FiltroEquipamentoRequest
    {
        public string? Nome { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? CategoriaId { get; set; }
        public bool? Ativo { get; set; }
    }
}