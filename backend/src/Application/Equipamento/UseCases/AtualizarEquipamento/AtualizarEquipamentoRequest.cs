using FluentValidation;
using Portal.Application.Equipamento.UseCases.CriarEquipamento;
using Portal.Domain.Base;

namespace Portal.Application.Equipamento.UseCases.AtualizarEquipamento;

public class AtualizarEquipamentoRequest : BaseRequest
{
    public Guid Id { get; set; }

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
    public bool Ativo { get; set; }


    public override bool IsValid()
    {
        var validator = new AtualizarEquipamentoValidator();
        var result = validator.Validate(this);
        return result.IsValid;
    }
}
