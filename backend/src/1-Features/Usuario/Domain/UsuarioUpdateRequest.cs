namespace Portal.Features.Usuario.Domain
{
    public class UsuarioUpdateRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public bool Bloqueado { get; set; }
    }
}
