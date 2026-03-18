using Portal.Dominio;

namespace Portal.Features.Auth.Domain.Requests
{
    public class LogoutRequest : BaseRequest
    {
        public string RefreshToken { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Validations.LogoutValidator();
            return Validate(this, validator);
        }
    }
}
