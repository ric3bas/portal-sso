using Microsoft.AspNetCore.Authorization;

namespace Portal.Dominio
{
    public class AuthorizeTenantIdAttribute : AuthorizeAttribute
    {
        public AuthorizeTenantIdAttribute() : base("TenantIdPolicy") { }
    }
}
