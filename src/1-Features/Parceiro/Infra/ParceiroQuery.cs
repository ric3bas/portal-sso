using Portal.Features.Parceiro.Domain;

namespace Portal.Features.Parceiro.Infra {
    public class ParceiroQuery
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;

        public ParceiroResponse ToResponse() => new ParceiroResponse
        {
            Nome = Nome,
            Descricao = Descricao,
            Ativo = Ativo
        };
    }
}