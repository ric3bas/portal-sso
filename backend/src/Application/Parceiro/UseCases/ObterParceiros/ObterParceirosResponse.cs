namespace Portal.Application.Parceiro.UseCases.ObterParceiros;

public class ObterParceirosResponse
{
    public Guid Id { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
