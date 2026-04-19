namespace Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;

public class ObterEquipamentosPorFiltroRequest
{
    public string? Nome { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool? Ativo { get; set; }
}
