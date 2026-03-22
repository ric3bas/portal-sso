namespace Portal.Features.Usuario.Infra
{
    public class TokenAtualizacaoQuery
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiraEm { get; set; }
        public bool Revogado { get; set; }
        public int UsuarioId { get; set; }
        public UsuarioQuery? Usuario { get; set; }
    }
}
