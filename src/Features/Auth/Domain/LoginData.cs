using Portal.Features.Auth.Domain.Responses;

namespace Portal.Features.Auth.Domain
{
    public class LoginData
    {
        public Dominio.Entities.UsuarioEntity? Usuario { get; set; }
        public UsuarioPerfilItemResponse? Perfil { get; set; }
        public IReadOnlyCollection<string> Escopos { get; set; } = [];
    }
}
