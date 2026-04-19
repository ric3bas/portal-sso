using FluentValidation;

namespace Portal.Application.Locacao.UseCases.AtualizarLocacao
{
    public class AtualizarLocacaoValidator : AbstractValidator<AtualizarLocacaoRequest>
    {
        public AtualizarLocacaoValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id da locação é obrigatório");

        }
    }
}
