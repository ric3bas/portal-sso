using FluentValidation;

namespace Portal.Features.Locacao.Domain.Validations
{
    public class LocacaoRequestValidator : AbstractValidator<LocacaoRequest>
    {
        public LocacaoRequestValidator()
        {
            RuleFor(x => x.ClienteId)
                .NotNull().NotEmpty().WithMessage("Cliente é obrigatório")
                .Must(BeValidGuid).WithMessage("Cliente inválido");

            RuleFor(x => x.EquipamentoId)
                .NotNull().NotEmpty().WithMessage("Equipamento é obrigatório")
                .Must(BeValidGuid).WithMessage("Equipamento inválido");

            RuleFor(x => x.DataRetirada)
                .NotEmpty().WithMessage("Data de retirada é obrigatória")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Data de retirada não pode ser anterior ao dia atual");

            RuleFor(x => x.PrevisaoDevolucao)
                .NotEmpty().WithMessage("Previsão de devolução é obrigatória")
                .GreaterThan(x => x.DataRetirada).WithMessage("Previsão de devolução deve ser posterior à data de retirada");

            RuleFor(x => x.ValorDiaria)
                .GreaterThan(0).WithMessage("Valor da diária deve ser maior que zero");

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");
        }

        private bool BeValidGuid(string? guid)
        {
            return Guid.TryParse(guid, out _);
        }
    }

    public class AtualizarLocacaoRequestValidator : AbstractValidator<AtualizarLocacaoRequest>
    {
        public AtualizarLocacaoRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().NotEmpty().WithMessage("Id da locação é obrigatório");

            RuleFor(x => x.ClienteId)
                .NotNull().NotEmpty().WithMessage("Cliente é obrigatório")
                .Must(BeValidGuid).WithMessage("Cliente inválido");

            RuleFor(x => x.EquipamentoId)
                .NotNull().NotEmpty().WithMessage("Equipamento é obrigatório")
                .Must(BeValidGuid).WithMessage("Equipamento inválido");

            RuleFor(x => x.DataRetirada)
                .NotEmpty().WithMessage("Data de retirada é obrigatória");

            RuleFor(x => x.PrevisaoDevolucao)
                .NotEmpty().WithMessage("Previsão de devolução é obrigatória")
                .GreaterThan(x => x.DataRetirada).WithMessage("Previsão de devolução deve ser posterior à data de retirada");

            RuleFor(x => x.ValorDiaria)
                .GreaterThan(0).WithMessage("Valor da diária deve ser maior que zero");

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");
        }

        private bool BeValidGuid(string? guid)
        {
            return Guid.TryParse(guid, out _);
        }
    }

    public class DevolverLocacaoRequestValidator : AbstractValidator<DevolverLocacaoRequest>
    {
        public DevolverLocacaoRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().NotEmpty().WithMessage("Id da locação é obrigatório");

            RuleFor(x => x.DataDevolucao)
                .NotEmpty().WithMessage("Data de devolução é obrigatória")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Data de devolução não pode ser futura");

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");
        }
    }
}