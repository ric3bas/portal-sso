using FluentValidation;
using Portal.Domain.Base;
using Portal.Features.Auth.Domain.Requests;

namespace Portal.Features.Usuario.Domain.Validations {
    public class RecuperarSenhaValidator: AbstractValidator<RecuperarSenhaRequest>
    {
        public RecuperarSenhaValidator()
        {
            RuleFor(x => x.Login).AplicaRegraCampoObrigatorio();
        }
    }   
}
