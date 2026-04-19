using FluentValidation;

namespace Portal.Application.Locacao.UseCases.CriarLocacao
{
    public class CriarLocacaoValidator : AbstractValidator<CriarLocacaoRequest>
    {
        public CriarLocacaoValidator()
        {
            RuleFor(x => x.ClienteId)
                .NotEmpty().WithMessage("Cliente é obrigatório");

            RuleFor(x => x.EquipamentoId)
                .NotEmpty().WithMessage("Equipamento é obrigatório");

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
    }
}
