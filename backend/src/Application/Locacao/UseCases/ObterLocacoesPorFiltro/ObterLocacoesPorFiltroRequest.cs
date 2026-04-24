using Portal.Domain.Common;
using Portal.Domain.Locacao;

namespace Portal.Application.Locacao.UseCases.ObterLocacoesPorFiltro;

public class ObterLocacoesPorFiltroRequest : PaginacaoFiltro
{
    public Guid? ClienteId { get; set; }
    public Guid? EquipamentoId { get; set; }
    public StatusLocacao? Status { get; set; }
    public DateTime? DataRetiradaInicio { get; set; }
    public DateTime? DataRetiradaFim { get; set; }
}
