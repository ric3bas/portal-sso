namespace Portal.Domain.Usuario;

public class UsuarioComPerfilQuery
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Parceiro { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool Bloqueado { get; set; }
}
