using FluentValidation;

namespace Portal.Application.Locacao.UseCases.DevolverLocacao
{
    public class DevolverLocacaoValidator : AbstractValidator<DevolverLocacaoRequest>
    {
        public DevolverLocacaoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id da locação é obrigatório");

            RuleFor(x => x.DataDevolucao)
                .NotEmpty().WithMessage("Data de devolução é obrigatória")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Data de devolução não pode ser futura");

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");
        }
    }
}
