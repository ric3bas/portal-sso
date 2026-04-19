using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.ValidarTokenRecuperacao
{
    public class ValidarTokenRecuperacaoValidator : AbstractValidator<ValidarTokenRecuperacaoRequest>
    {
        public ValidarTokenRecuperacaoValidator()
        {
            RuleFor(x => x.Token).AplicaRegraCampoObrigatorio();
        }
    }
}
