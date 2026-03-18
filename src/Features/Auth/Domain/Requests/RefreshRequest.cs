using Portal.Dominio;

namespace Portal.Features.Auth.Domain.Requests
{
    public class RefreshRequest : BaseRequest
    {
        public string RefreshToken { get; set; } = string.Empty;

        public override bool IsValid()
        {
            var validator = new Validations.RefreshValidator();
            return Validate(this, validator);
        }
    }
}
