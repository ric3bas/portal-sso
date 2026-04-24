namespace Portal.Application.Usuario.UseCases.AtualizarUsuario;

public class AtualizarUsuarioRequest
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool Bloqueado { get; set; }
}
