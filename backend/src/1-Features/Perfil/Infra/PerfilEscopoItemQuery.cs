using Portal.Features.Perfil.Domain;

namespace Portal.Features.Perfil.Infra
{
    public class PerfilEscopoItemQuery
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        public PerfilEscopoItemResponse ToResponse()
        {
            return new PerfilEscopoItemResponse
            {
                Id = Id,
                Nome = Nome
            };
        }
    }
}
