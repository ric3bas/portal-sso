namespace Portal.Application.Usuario.UseCases.RegistrarUsuario;

public class RegistrarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public int PerfilId { get; set; }
    public string? ParceiroId { get; set; }
}
