using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Equipamento.UseCases.AtualizarEquipamento;

public class AtualizarEquipamentoRequest : BaseRequest
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string CategoriaId { get; set; } = string.Empty;
    public int QuantidadeEstoque { get; set; }
    public decimal PrecoDiaria { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public string? Descricao { get; set; }
    public string? ObservacaoInternas { get; set; }
    public bool Ativo { get; set; }


    public override bool IsValid()
    {
        var validator = new InlineValidator<AtualizarEquipamentoRequest>();
        validator.RuleFor(x => x.Id).NotEmpty();
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(2).MaximumLength(200);
        validator.RuleFor(x => x.CategoriaId).NotEmpty();
        validator.RuleFor(x => x.QuantidadeEstoque).GreaterThanOrEqualTo(0);
        validator.RuleFor(x => x.PrecoDiaria).GreaterThan(0);
        validator.RuleFor(x => x.Marca).NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.Modelo).NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.NumeroSerie).NotEmpty().MaximumLength(50);
        validator.RuleFor(x => x.AnoFabricacao).GreaterThan(1900).LessThanOrEqualTo(DateTime.Now.Year + 1);
        validator.RuleFor(x => x.Descricao).MaximumLength(500);
        validator.RuleFor(x => x.ObservacaoInternas).MaximumLength(1000);
        return Validate(this, validator);
    }
}
