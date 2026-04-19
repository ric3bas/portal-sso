namespace Portal.Application.Categoria.UseCases.CriarCategoria;

public class CriarCategoriaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public Guid ParceiroId { get; set; }
}
