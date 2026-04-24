using Portal.Domain.Common;

namespace Portal.Application.Usuario.UseCases.ObterUsuarios;

public class ObterUsuariosRequest : PaginacaoFiltro
{
    public string? ParceiroId { get; set; }
}

