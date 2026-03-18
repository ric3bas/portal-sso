namespace Portal.Dominio.Entities
{
    public class UsuarioEntity
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public Guid ParceiroId { get; set; }
        public int? PerfilId { get; set; }
    }
}
