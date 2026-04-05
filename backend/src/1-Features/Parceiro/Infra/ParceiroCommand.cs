namespace Portal.Features.Parceiro.Infra
{
    public class ParceiroCommand
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativo { get; set; } = true;
    }
}