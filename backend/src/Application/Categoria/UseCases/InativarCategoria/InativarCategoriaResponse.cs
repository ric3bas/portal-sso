namespace Portal.Application.Categoria.UseCases.InativarCategoria;

public class InativarCategoriaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public Guid ParceiroId { get; set; }
}
