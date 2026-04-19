
using Portal.Application.Perfil.UseCases.ObterPerfisParaCombo;

namespace Portal.Application.Perfil.UseCases.ObterPerfisComEscopo;

public class ObterPerfisComEscopoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<ObterPerfisParaComboResponse> Escopos { get; set; } = [];
}
