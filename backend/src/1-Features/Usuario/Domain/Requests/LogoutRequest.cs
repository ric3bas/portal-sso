using Portal.Domain.Base;

namespace Portal.Features.Auth.Domain.Requests
{
    public class LogoutRequest : BaseRequest
    {
        public string RefreshToken { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Usuario.Domain.Validations.LogoutValidator();
            return Validate(this, validator);
        }
    }
}
