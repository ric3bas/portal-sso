namespace Portal.Domain.Perfil;

public class PerfilQuery
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<PerfilEscopoQuery> Escopos { get; set; } = [];
}
