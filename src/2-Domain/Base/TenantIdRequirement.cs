using Microsoft.AspNetCore.Authorization;

namespace Portal.Domain.Base
{
    public class TenantIdRequirement : IAuthorizationRequirement { }

    public class TenantIdHandler : AuthorizationHandler<TenantIdRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantIdRequirement requirement)
        {
            var hasTenant = context.User.HasClaim(c => c.Type == "tenantId" && !string.IsNullOrEmpty(c.Value));
            if (hasTenant)
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
