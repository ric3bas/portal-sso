namespace Portal.Features.Parceiro.Domain
{
    public class ParceiroResponse
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
    }
}
