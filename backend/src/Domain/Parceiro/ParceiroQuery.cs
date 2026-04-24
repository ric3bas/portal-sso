using Portal.Application.Parceiro.UseCases.ObterParceiros;

namespace Portal.Domain.Parceiro
{
    public class ParceiroQuery
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? CorPrimaria { get; set; }
        public string? CorSecundaria { get; set; }
        public bool Ativo { get; set; } = true;

        public TResponse ToResponse<TResponse>() where TResponse : ObterParceirosResponse, new()
        {
            return new TResponse
            {
                Id = Id,
                Nome = Nome,
                Descricao = Descricao,
                CorPrimaria = CorPrimaria,
                CorSecundaria = CorSecundaria,
                Ativo = Ativo
            };
        }

    }
}
