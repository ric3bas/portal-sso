namespace Portal.Domain.Common;

public sealed class ResultadoPaginado<T>
{
    public IEnumerable<T> Itens { get; }
    public int TotalRegistros { get; }
    public int NumeroPagina { get; }
    public int TamanhoPagina { get; }

    public ResultadoPaginado(IEnumerable<T> itens, int totalRegistros, int numeroPagina, int tamanhoPagina)
    {
        Itens = itens;
        TotalRegistros = totalRegistros;
        NumeroPagina = numeroPagina;
        TamanhoPagina = tamanhoPagina;
    }
}
