namespace Portal.Features.Usuario.Infra
{
    public class RecuperacaoSenhaCommand
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
        public bool Usado { get; set; }
    }
}
