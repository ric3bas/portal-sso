using Portal.Domain.Base;

namespace Portal.Application.Equipamento.UseCases.CriarEquipamento;

public class CriarEquipamentoRequest : BaseRequest
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
        var validator = new CriarEquipamentoValidator();
        var result = validator.Validate(this);
        return result.IsValid;
    }
}
