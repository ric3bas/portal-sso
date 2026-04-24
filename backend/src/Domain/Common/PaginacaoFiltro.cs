namespace Portal.Domain.Common;

public enum Direcao
{
    Asc = 0,
    Desc = 1
}

public abstract class PaginacaoFiltro
{
    private const int PaginaPadrao = 1;
    private const int TamanhoPaginaPadrao = 20;
    private const int TamanhoPaginaMaximo = 100;

    private string _direcao = "asc";
    private int _pagina = PaginaPadrao;
    private int _tamanhoPagina = TamanhoPaginaPadrao;

    public string Direcao
    {
        get => _direcao;
        set => _direcao = string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
    }

    public Direcao DirecaoEnum => PaginacaoHelper.ParseDirecao(_direcao);

    public int Pagina
    {
        get => _pagina;
        set => _pagina = value <= 0 ? PaginaPadrao : value;
    }

    public int TamanhoPagina
    {
        get => _tamanhoPagina;
        set
        {
            if (value <= 0)
            {
                _tamanhoPagina = TamanhoPaginaPadrao;
                return;
            }

            _tamanhoPagina = Math.Min(value, TamanhoPaginaMaximo);
        }
    }
}

public static class PaginacaoHelper
{
    public static Direcao ParseDirecao(string? direcao) =>
        string.Equals(direcao, "desc", StringComparison.OrdinalIgnoreCase)
            ? Direcao.Desc
            : Direcao.Asc;

    public static PaginacaoInfo Criar(Direcao direcao, int pagina, int tamanhoPagina)
    {
        var ordemSql = direcao == Direcao.Desc ? "DESC" : "ASC";
        var paginaNormalizada = pagina <= 0 ? 1 : pagina;
        var tamanhoNormalizado = tamanhoPagina <= 0 ? 20 : Math.Min(tamanhoPagina, 100);
        var offset = (paginaNormalizada - 1) * tamanhoNormalizado;

        return new PaginacaoInfo(direcao, ordemSql, paginaNormalizada, tamanhoNormalizado, offset);
    }

}

public sealed record PaginacaoInfo(Direcao Direcao, string OrdemSql, int Pagina, int TamanhoPagina, int Offset);
