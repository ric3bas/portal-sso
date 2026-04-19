namespace Portal.Domain.Usuario;

public class TokenAtualizacaoCommand
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public bool Revogado { get; set; }
    public int UsuarioId { get; set; }
    public string IpUsuario { get; set; } = string.Empty;
    public DateTime LogadoEm { get; set; }
}
