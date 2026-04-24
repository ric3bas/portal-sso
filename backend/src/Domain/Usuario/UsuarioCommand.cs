namespace Portal.Domain.Usuario;

public class UsuarioCommand
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public Guid ParceiroId { get; set; }
    public int PerfilId { get; set; }
    public int TentativasLogin { get; set; }
    public bool Bloqueado { get; set; }
    public bool Ativo { get; set; }
}
