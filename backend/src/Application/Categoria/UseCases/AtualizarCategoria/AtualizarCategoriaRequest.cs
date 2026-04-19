namespace Portal.Application.Categoria.UseCases.AtualizarCategoria;

public class AtualizarCategoriaRequest
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool? Ativo { get; set; }
}
