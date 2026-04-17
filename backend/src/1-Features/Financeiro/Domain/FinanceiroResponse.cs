namespace Portal.Features.Financeiro.Domain
{
    public class FinanceiroResponse
    {
        public Guid Id { get; set; }
        public Guid LocacaoId { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public Guid EquipamentoId { get; set; }
        public string EquipamentoNome { get; set; } = string.Empty;
        public DateTime DataRetirada { get; set; }
        public DateTime DataDevolucao { get; set; }
        public int DiasLocados { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataLancamento { get; set; }
    }

    internal static class FinanceiroExtensions
    {
        internal static FinanceiroResponse ToResponse(this FinanceiroEntity financeiro)
        {
            return new FinanceiroResponse
            {
                Id = financeiro.Id,
                LocacaoId = financeiro.LocacaoId,
                ClienteId = financeiro.ClienteId,
                ClienteNome = financeiro.ClienteNome ?? string.Empty,
                EquipamentoId = financeiro.EquipamentoId,
                EquipamentoNome = financeiro.EquipamentoNome ?? string.Empty,
                DataRetirada = financeiro.DataRetirada,
                DataDevolucao = financeiro.DataDevolucao,
                DiasLocados = financeiro.DiasLocados,
                ValorDiaria = financeiro.ValorDiaria,
                ValorTotal = financeiro.ValorTotal,
                DataLancamento = financeiro.DataLancamento
            };
        }
    }

    public class FinanceiroEntity
    {
        public Guid Id { get; set; }
        public Guid LocacaoId { get; set; }
        public Guid ClienteId { get; set; }
        public string? ClienteNome { get; set; }
        public Guid EquipamentoId { get; set; }
        public string? EquipamentoNome { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime DataDevolucao { get; set; }
        public int DiasLocados { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataLancamento { get; set; }
        public Guid ParceiroId { get; set; }
    }
}