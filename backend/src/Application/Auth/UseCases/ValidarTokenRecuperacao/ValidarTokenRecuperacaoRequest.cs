using FluentValidation;
using Microsoft.Extensions.Options;
using Portal.Application.Auth.UseCases.TrocarSenha;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.ValidarTokenRecuperacao
{
    public class ValidarTokenRecuperacaoRequest : BaseRequest
    {
        public string Token { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new InlineValidator<ValidarTokenRecuperacaoRequest>();

            validator.RuleFor(x => x.Token).AplicaRegraCampoObrigatorio();
            return Validate(this, validator);
        }
    }
}
