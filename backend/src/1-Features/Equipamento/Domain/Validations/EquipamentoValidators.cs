using FluentValidation;

namespace Portal.Features.Equipamento.Domain.Validations
{
    public class EquipamentoRequestValidator : AbstractValidator<EquipamentoRequest>
    {
        public EquipamentoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotNull().NotEmpty().WithMessage("Nome do equipamento é obrigatório")
                .MinimumLength(2).WithMessage("Nome do equipamento deve ter pelo menos 2 caracteres")
                .MaximumLength(200).WithMessage("Nome do equipamento não pode ter mais de 200 caracteres");

            RuleFor(x => x.CategoriaId)
                .NotNull().NotEmpty().WithMessage("Categoria é obrigatória")
                .Must(BeValidGuid).WithMessage("Categoria inválida");

            RuleFor(x => x.QuantidadeEstoque)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque deve ser maior ou igual a zero");

            RuleFor(x => x.PrecoDiaria)
                .GreaterThan(0).WithMessage("Preço da diária deve ser maior que zero");

            RuleFor(x => x.Marca)
                .NotNull().NotEmpty().WithMessage("Marca é obrigatória")
                .MaximumLength(100).WithMessage("Marca não pode ter mais de 100 caracteres");

            RuleFor(x => x.Modelo)
                .NotNull().NotEmpty().WithMessage("Modelo é obrigatório")
                .MaximumLength(100).WithMessage("Modelo não pode ter mais de 100 caracteres");

            RuleFor(x => x.NumeroSerie)
                .NotNull().NotEmpty().WithMessage("Número de série é obrigatório")
                .MaximumLength(50).WithMessage("Número de série não pode ter mais de 50 caracteres");

            RuleFor(x => x.AnoFabricacao)
                .GreaterThan(1900).WithMessage("Ano de fabricação deve ser maior que 1900")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Ano de fabricação não pode ser futuro");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição não pode ter mais de 500 caracteres");

            RuleFor(x => x.ObservacaoInternas)
                .MaximumLength(1000).WithMessage("Observações internas não podem ter mais de 1000 caracteres");
        }

        private bool BeValidGuid(string? guid)
        {
            return Guid.TryParse(guid, out _);
        }
    }

    public class AtualizarEquipamentoRequestValidator : AbstractValidator<AtualizarEquipamentoRequest>
    {
        public AtualizarEquipamentoRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().NotEmpty().WithMessage("Id do equipamento é obrigatório");

            RuleFor(x => x.Nome)
                .NotNull().NotEmpty().WithMessage("Nome do equipamento é obrigatório")
                .MinimumLength(2).WithMessage("Nome do equipamento deve ter pelo menos 2 caracteres")
                .MaximumLength(200).WithMessage("Nome do equipamento não pode ter mais de 200 caracteres");

            RuleFor(x => x.CategoriaId)
                .NotNull().NotEmpty().WithMessage("Categoria é obrigatória")
                .Must(BeValidGuid).WithMessage("Categoria inválida");

            RuleFor(x => x.QuantidadeEstoque)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque deve ser maior ou igual a zero");

            RuleFor(x => x.PrecoDiaria)
                .GreaterThan(0).WithMessage("Preço da diária deve ser maior que zero");

            RuleFor(x => x.Marca)
                .NotNull().NotEmpty().WithMessage("Marca é obrigatória")
                .MaximumLength(100).WithMessage("Marca não pode ter mais de 100 caracteres");

            RuleFor(x => x.Modelo)
                .NotNull().NotEmpty().WithMessage("Modelo é obrigatório")
                .MaximumLength(100).WithMessage("Modelo não pode ter mais de 100 caracteres");

            RuleFor(x => x.NumeroSerie)
                .NotNull().NotEmpty().WithMessage("Número de série é obrigatório")
                .MaximumLength(50).WithMessage("Número de série não pode ter mais de 50 caracteres");

            RuleFor(x => x.AnoFabricacao)
                .GreaterThan(1900).WithMessage("Ano de fabricação deve ser maior que 1900")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Ano de fabricação não pode ser futuro");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição não pode ter mais de 500 caracteres");

            RuleFor(x => x.ObservacaoInternas)
                .MaximumLength(1000).WithMessage("Observações internas não podem ter mais de 1000 caracteres");
        }

        private bool BeValidGuid(string? guid)
        {
            return Guid.TryParse(guid, out _);
        }
    }
}