using Portal.Features.Escopo.Domain;

namespace Portal.Features.Escopo.Infra
{
    public class EscopoQuery
    {
        public int Id { get; set; }
        public string? Nome { get; set; }

        public EscopoResponse ToResponse()
        {
            return new EscopoResponse
            {
                Id = this.Id,
                Nome = this.Nome
            };
        }
    }
}
