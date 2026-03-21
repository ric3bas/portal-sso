namespace Portal.Dominio.Entities
{
    public class TokenAtualizacaoEntity
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiraEm { get; set; }
        public bool Revogado { get; set; }
        public int UsuarioId { get; set; }
        public UsuarioEntity? Usuario { get; set; }
    }
}
