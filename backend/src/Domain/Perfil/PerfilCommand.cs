namespace Portal.Domain.Perfil;

public class PerfilCommand
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<PerfilEscopoCommand> Escopos { get; set; } = [];
}
