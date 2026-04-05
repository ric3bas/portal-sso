using Portal.Features.Usuario.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Portal.Domain.Base
{
    public class JwtRevocationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtRevocationMiddleware(RequestDelegate next){ _next = next;}

        public async Task Invoke(HttpContext context)
        {
            var _authService = context.RequestServices.GetRequiredService<IAuthService>();

            var token = context.User.FindFirst("refresh-token")?.Value;
            var sessao = await _authService.ObterTokenSessaoAsync(token ?? string.Empty);

            if (sessao != null && sessao.Revogado)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                return;
            }

            await _next(context);
        }
    }
}
