using Portal.Application.Escopo.UseCases.ObterEscopos;
using Portal.Application.Escopo.UseCases.ObterEscopoPorId;

namespace Portal.Domain.Escopo;

public class EscopoQuery
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ObterEscoposResponse ToResponse()
    {
        return new ObterEscoposResponse
        {
            Id = Id,
            Nome = Nome
        };
    }

    public ObterEscopoPorIdResponse ToResponsePorId()
    {
        return new ObterEscopoPorIdResponse
        {
            Id = Id,
            Nome = Nome
        };
    }
}
