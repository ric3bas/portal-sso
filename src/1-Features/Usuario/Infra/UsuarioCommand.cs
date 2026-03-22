namespace Portal.Features.Usuario.Infra

{
    public class UsuarioCommand
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public Guid ParceiroId { get; set; }
        public int? PerfilId { get; set; }
        public int TentativasLogin { get; set; }
        public DateTime? UltimoErroLogin { get; set; }
        public bool Bloqueado { get; set; }
    }
}
