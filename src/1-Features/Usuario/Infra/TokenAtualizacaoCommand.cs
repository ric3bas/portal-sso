
namespace Portal.Features.Usuario.Infra
{
    public class TokenAtualizacaoCommand
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiraEm { get; set; }
        public bool Revogado { get; set; }
        public int UsuarioId { get; set; }
        public UsuarioCommand? Usuario { get; set; }
    }
}
