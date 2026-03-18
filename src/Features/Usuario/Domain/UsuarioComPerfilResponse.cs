namespace Portal.Features.Usuario.Domain
{
    public class UsuarioComPerfilResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid ParceiroId { get; set; }
        //public List<UsuarioPerfilItemResponse> Perfis { get; set; } = new List<UsuarioPerfilItemResponse>();
    }

    //public class UsuarioPerfilItemResponse
    //{
    //    public int Id { get; set; }
    //    public string Nome { get; set; } = string.Empty;
    //}
}
