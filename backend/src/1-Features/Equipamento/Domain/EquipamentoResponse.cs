namespace Portal.Features.Equipamento.Domain
{
    public class EquipamentoResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Guid CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public int QuantidadeEstoque { get; set; }
        public decimal PrecoDiaria { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int AnoFabricacao { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string ObservacaoInternas { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    internal static class EquipamentoExtensions
    {
        internal static EquipamentoResponse ToResponse(this EquipamentoEntity equipamento)
        {
            return new EquipamentoResponse
            {
                Id = equipamento.Id,
                Nome = equipamento.Nome,
                CategoriaId = equipamento.CategoriaId,
                CategoriaNome = equipamento.CategoriaNome ?? string.Empty,
                QuantidadeEstoque = equipamento.QuantidadeEstoque,
                PrecoDiaria = equipamento.PrecoDiaria,
                Marca = equipamento.Marca,
                Modelo = equipamento.Modelo,
                NumeroSerie = equipamento.NumeroSerie,
                AnoFabricacao = equipamento.AnoFabricacao,
                Descricao = equipamento.Descricao,
                ObservacaoInternas = equipamento.ObservacaoInternas,
                Ativo = equipamento.Ativo
            };
        }
    }

    public class EquipamentoEntity
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Guid CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
        public int QuantidadeEstoque { get; set; }
        public decimal PrecoDiaria { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public int AnoFabricacao { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string ObservacaoInternas { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public Guid ParceiroId { get; set; }
    }
}