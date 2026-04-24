using Portal.Application.Categoria.UseCases.ObterCategoriaPorId;
using Portal.Application.Categoria.UseCases.ObterCategorias;
using Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro;

namespace Portal.Domain.Categoria;

public class CategoriaQuery
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public Guid ParceiroId { get; set; }

    public ObterCategoriasResponse ToResponse()
    {
        return new ObterCategoriasResponse
        {
            Id = Id,
            Nome = Nome,
            Ativo = Ativo,
            ParceiroId = ParceiroId
        };
    }

    public ObterCategoriasPorFiltroResponse ToResponsePorFiltro()
    {
        return new ObterCategoriasPorFiltroResponse
        {
            Id = Id,
            Nome = Nome,
            Ativo = Ativo,
            ParceiroId = ParceiroId
        };
    }

    public ObterCategoriaPorIdResponse ToResponsePorId()
    {
        return new ObterCategoriaPorIdResponse
        {
            Id = Id,
            Nome = Nome,
            Ativo = Ativo,
            ParceiroId = ParceiroId
        };
    }
}
