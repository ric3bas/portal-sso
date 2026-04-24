namespace Portal.Domain.Usuario;

public class UsuarioPerfilQuery
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool IsMaster { get; set; }
}
