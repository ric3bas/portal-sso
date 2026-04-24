using Portal.Domain.Usuario.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Portal.Domain.Base
{
    public class JwtRevocationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtRevocationMiddleware(RequestDelegate next){ _next = next;}

        public async Task Invoke(HttpContext context)
        {
            var tokenRepo = context.RequestServices.GetRequiredService<ITokenAtualizacaoRepository>();

            var token = context.User.FindFirst("refresh-token")?.Value;
            var sessao = await tokenRepo.ObterPorTokenAsync(token ?? string.Empty);

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
