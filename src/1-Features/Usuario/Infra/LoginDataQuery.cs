namespace Portal.Features.Usuario.Infra
{
    public class LoginDataQuery
    {
        public UsuarioQuery? Usuario { get; set; }
        public UsuarioPerfilItemQuery? Perfil { get; set; }
        public IReadOnlyCollection<string> Escopos { get; set; } = [];
    }
}
