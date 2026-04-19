namespace Portal.Domain.Usuario;

public class LoginDataQuery
{
    public UsuarioQuery? Usuario { get; set; }
    public UsuarioPerfilQuery? Perfil { get; set; }
    public List<string> Escopos { get; set; } = [];
}
