using Portal.Domain.Base;

namespace Portal.Application.Auth.UseCases.RefreshToken;

public class RefreshTokenRequest : BaseRequest
{
    public string RefreshToken { get; set; } = string.Empty;

    public override bool IsValid()
    {
        var validator = new RefreshTokenValidator();
        return Validate(this, validator);
    }
}
