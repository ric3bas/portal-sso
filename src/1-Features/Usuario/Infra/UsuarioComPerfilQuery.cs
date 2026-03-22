namespace Portal.Features.Usuario.Infra
{
    public class UsuarioComPerfilQuery
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid ParceiroId { get; set; }
    }
}
