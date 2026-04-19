using FluentValidation;

namespace Portal.Application.Equipamento.UseCases.CriarEquipamento
{
    public class CriarEquipamentoValidator : AbstractValidator<CriarEquipamentoRequest>
    {
        public CriarEquipamentoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome do equipamento é obrigatório")
                .MinimumLength(2).WithMessage("Nome do equipamento deve ter pelo menos 2 caracteres")
                .MaximumLength(200).WithMessage("Nome do equipamento não pode ter mais de 200 caracteres");

            RuleFor(x => x.CategoriaId)
                .NotEmpty().WithMessage("Categoria é obrigatória");

            RuleFor(x => x.QuantidadeEstoque)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque deve ser maior ou igual a zero");

            RuleFor(x => x.PrecoDiaria)
                .GreaterThan(0).WithMessage("Preço da diária deve ser maior que zero");

            RuleFor(x => x.Marca)
                .NotEmpty().WithMessage("Marca é obrigatória")
                .MaximumLength(100).WithMessage("Marca não pode ter mais de 100 caracteres");

            RuleFor(x => x.Modelo)
                .NotEmpty().WithMessage("Modelo é obrigatório")
                .MaximumLength(100).WithMessage("Modelo não pode ter mais de 100 caracteres");

            RuleFor(x => x.NumeroSerie)
                .NotEmpty().WithMessage("Número de série é obrigatório")
                .MaximumLength(50).WithMessage("Número de série não pode ter mais de 50 caracteres");

            RuleFor(x => x.AnoFabricacao)
                .GreaterThan(1900).WithMessage("Ano de fabricação deve ser maior que 1900")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Ano de fabricação não pode ser futuro");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição não pode ter mais de 500 caracteres");

            RuleFor(x => x.ObservacaoInternas)
                .MaximumLength(1000).WithMessage("Observações internas não podem ter mais de 1000 caracteres");
        }
    }
}
