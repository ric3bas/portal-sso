using Portal.Application.Categoria.UseCases.ObterCategorias;

namespace Portal.Application.Categoria.UseCases.ObterCategoriaPorId;

public class ObterCategoriaPorIdResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public Guid ParceiroId { get; set; }
}
