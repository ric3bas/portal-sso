using FluentValidation;
using Portal.Domain.Base;

namespace Portal.Application.Usuario.UseCases.CriarUsuario;

public class CriarUsuarioRequest : BaseRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public int PerfilId { get; set; }
    public string? ParceiroId { get; set; }

    public override bool IsValid()
    {
        var validator = new InlineValidator<CriarUsuarioRequest>();
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(3).MaximumLength(100);
        validator.RuleFor(x => x.Login).NotEmpty().MinimumLength(3).MaximumLength(50);
        validator.RuleFor(x => x.Senha).NotEmpty().MinimumLength(6).MaximumLength(20).Matches("[A-Z]").Matches("[0-9]");
        validator.RuleFor(x => x.Email).NotEmpty().EmailAddress();
        validator.RuleFor(x => x.PerfilId).GreaterThan(0);
        return Validate(this, validator);
    }
}

