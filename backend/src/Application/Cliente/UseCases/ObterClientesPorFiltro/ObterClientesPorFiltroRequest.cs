namespace Portal.Application.Cliente.UseCases.ObterClientesPorFiltro;

public class ObterClientesPorFiltroRequest
{
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
}
