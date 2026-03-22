using Portal.Features.Perfil.Domain;

namespace Portal.Features.Perfil.Infra
{
    public class PerfilComEscopoQuery
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<PerfilEscopoItemQuery> Escopos { get; set; } = new List<PerfilEscopoItemQuery>();

        public PerfilComEscopoResponse ToResponse()
        {
            return new PerfilComEscopoResponse
            {
                Id = this.Id,
                Nome = this.Nome,
                Escopos = this.Escopos.Select(e => e.ToResponse()).ToList()
            };
        }
    }
}
