using Portal.Application.Perfil.UseCases.ObterPerfisComEscopo;
using Portal.Application.Perfil.UseCases.ObterPerfilPorId;
using Portal.Application.Perfil.UseCases.ObterPerfisParaCombo;

namespace Portal.Domain.Perfil;

public class PerfilQuery
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<PerfilEscopoQuery> Escopos { get; set; } = [];

    public ObterPerfisComEscopoResponse ToResponse()
    {
        return new ObterPerfisComEscopoResponse
        {
            Id = Id,
            Nome = Nome,
            Escopos = Escopos.Select(x => new ObterPerfisParaComboResponse
            {
                Id = x.Id,
                Nome = x.Nome
            }).ToList()
        };
    }

    public ObterPerfisParaComboResponse ToResponseParaCombo()
    {
        return new ObterPerfisParaComboResponse
        {
            Id = Id,
            Nome = Nome
        };
    }

    public ObterPerfilPorIdResponse ToResponsePorId()
    {
        return new ObterPerfilPorIdResponse
        {
            Id = Id,
            Nome = Nome,
            Escopos = Escopos.Select(x => new ObterPerfisParaComboResponse
            {
                Id = x.Id,
                Nome = x.Nome
            }).ToList()
        };
    }
}
