namespace Portal.Features.Locacao.Domain
{
    public enum StatusLocacao
    {
        Ativa = 1,
        Devolvida = 2,
        Atrasada = 3,
        Cancelada = 4
    }

    public class LocacaoResponse
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public Guid EquipamentoId { get; set; }
        public string EquipamentoNome { get; set; } = string.Empty;
        public StatusLocacao Status { get; set; }
        public string StatusDescricao { get; set; } = string.Empty;
        public DateTime DataRetirada { get; set; }
        public DateTime PrevisaoDevolucao { get; set; }
        public DateTime? DataDevolucaoReal { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal? ValorTotal { get; set; }
        public int? DiasLocados { get; set; }
        public string? Observacao { get; set; }
    }

    internal static class LocacaoExtensions
    {
        internal static LocacaoResponse ToResponse(this LocacaoEntity locacao)
        {
            return new LocacaoResponse
            {
                Id = locacao.Id,
                ClienteId = locacao.ClienteId,
                ClienteNome = locacao.ClienteNome ?? string.Empty,
                EquipamentoId = locacao.EquipamentoId,
                EquipamentoNome = locacao.EquipamentoNome ?? string.Empty,
                Status = locacao.Status,
                StatusDescricao = locacao.Status.ToString(),
                DataRetirada = locacao.DataRetirada,
                PrevisaoDevolucao = locacao.PrevisaoDevolucao,
                DataDevolucaoReal = locacao.DataDevolucaoReal,
                ValorDiaria = locacao.ValorDiaria,
                ValorTotal = locacao.ValorTotal,
                DiasLocados = locacao.DiasLocados,
                Observacao = locacao.Observacao
            };
        }
    }

    public class LocacaoEntity
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string? ClienteNome { get; set; }
        public Guid EquipamentoId { get; set; }
        public string? EquipamentoNome { get; set; }
        public StatusLocacao Status { get; set; }
        public DateTime DataRetirada { get; set; }
        public DateTime PrevisaoDevolucao { get; set; }
        public DateTime? DataDevolucaoReal { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal? ValorTotal { get; set; }
        public int? DiasLocados { get; set; }
        public string? Observacao { get; set; }
        public Guid ParceiroId { get; set; }
    }
}