namespace Portal.Domain.Common;

public sealed class TabelaPaginadaResponse<T>
{
    public IEnumerable<T> Dados { get; init; } = [];
    public int RegistrosTotal { get; init; }
    public int RegistrosFiltrados { get; init; }
    public int Pagina { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalPaginas { get; init; }

    public static TabelaPaginadaResponse<T> Criar(
        IEnumerable<T> dados,
        int registrosTotal,
        int registrosFiltrados,
        int pagina,
        int tamanhoPagina)
    {
        var paginaNormalizada = pagina <= 0 ? 1 : pagina;
        var tamanhoNormalizado = tamanhoPagina <= 0 ? 20 : tamanhoPagina;
        var totalPaginas = registrosFiltrados <= 0
            ? 0
            : (int)Math.Ceiling(registrosFiltrados / (double)tamanhoNormalizado);

        return new TabelaPaginadaResponse<T>
        {
            Dados = dados,
            RegistrosTotal = registrosTotal,
            RegistrosFiltrados = registrosFiltrados,
            Pagina = paginaNormalizada,
            TamanhoPagina = tamanhoNormalizado,
            TotalPaginas = totalPaginas
        };
    }
}
