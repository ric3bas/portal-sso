using Microsoft.AspNetCore.Authorization;

namespace Portal.Domain.Base
{
    public class AuthorizeTenantIdAttribute : AuthorizeAttribute
    {
        public AuthorizeTenantIdAttribute() : base("TenantIdPolicy") { }
    }
}
