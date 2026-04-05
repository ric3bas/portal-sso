using Portal.Features.Perfil.Domain;

namespace Portal.Features.Perfil.Infra
{
    public class PerfilQuery
    {
        public int Id { get; set; }
        public string? Nome { get; set; }

        public PerfilResponse ToResponse()
        {
            return new PerfilResponse
            {
                Id = this.Id,
                Nome = this.Nome
            };
        }
    }
}
