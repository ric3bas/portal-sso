namespace Portal.Application.Categoria.UseCases.AtualizarCategoria;

public class AtualizarCategoriaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public Guid ParceiroId { get; set; }
}
