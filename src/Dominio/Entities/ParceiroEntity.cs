using Portal.Features.Parceiro.Domain;

namespace Portal.Domain.Entities {
    public class ParceiroEntity {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;

        public ParceiroResponse ToResponse() {
            return new ParceiroResponse {
                Id = this.Id,
                Nome = this.Nome,
                Descricao = this.Descricao,
                Ativo = this.Ativo
            };
        }
    }
}