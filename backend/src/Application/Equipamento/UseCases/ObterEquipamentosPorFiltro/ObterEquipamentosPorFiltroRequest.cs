using Portal.Domain.Common;

namespace Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;

public class ObterEquipamentosPorFiltroRequest : PaginacaoFiltro
{
    public string? Nome { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool? Ativo { get; set; }
}
