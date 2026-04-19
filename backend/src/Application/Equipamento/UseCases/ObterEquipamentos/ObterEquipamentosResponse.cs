
namespace Portal.Application.Equipamento.UseCases.ObterEquipamentos;

public class ObterEquipamentosResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
    public decimal PrecoDiaria { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public string? Descricao { get; set; }
    public string? ObservacaoInternas { get; set; }
    public bool Ativo { get; set; }
}
