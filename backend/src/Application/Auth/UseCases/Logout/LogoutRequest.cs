using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.Logout;

public class LogoutRequest : BaseRequest
{
    public string RefreshToken { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new LogoutValidator();
        return Validate(this, validator);
    }
}
