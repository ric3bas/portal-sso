namespace Portal.Application.Perfil.UseCases.VincularEscoposPerfil;

public class VincularEscoposPerfilRequest
{
    public int PerfilId { get; set; }
    public List<int> EscopoIds { get; set; } = [];
}
