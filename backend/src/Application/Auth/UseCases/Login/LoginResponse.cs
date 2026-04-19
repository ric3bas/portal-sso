namespace Portal.Application.Auth.UseCases.Login
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string ExpireInMinutes { get; set; } = string.Empty;
    }
}
