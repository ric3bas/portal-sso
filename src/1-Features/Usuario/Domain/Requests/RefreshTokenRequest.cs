using Portal.Domain.Base;

namespace Portal.Features.Auth.Domain.Requests
{
    public class RefreshTokenRequest : BaseRequest
    {
        public string RefreshToken { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Usuario.Domain.Validations.RefreshTokenValidator();
            return Validate(this, validator);
        }
    }
}
