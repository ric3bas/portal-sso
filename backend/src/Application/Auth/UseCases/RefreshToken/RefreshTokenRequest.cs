using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.RefreshToken;

public class RefreshTokenRequest : BaseRequest
{
    public string RefreshToken { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new InlineValidator<RefreshTokenRequest>();
        validator.RuleFor(x => x.RefreshToken).AplicaRegraCampoObrigatorio();
        return Validate(this, validator);
    }
}
