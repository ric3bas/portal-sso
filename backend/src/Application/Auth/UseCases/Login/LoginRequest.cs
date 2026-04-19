using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.Login;

public class LoginRequest : BaseRequest
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string IpUsuario { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new LoginRequestValidator();
        return Validate(this, validator);
    }
}
