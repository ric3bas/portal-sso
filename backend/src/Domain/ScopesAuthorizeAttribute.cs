using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Portal.Domain
{
    public class ScopesAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] _requiredScopes;

        public ScopesAuthorizeAttribute(params string[] scopes)
        {
            _requiredScopes = scopes;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool permissao = context.HttpContext.User.Claims.Any(c => _requiredScopes.Contains(c.Value));
            if (!permissao)
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
        }
    }
}
