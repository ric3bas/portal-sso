namespace Portal.WebApi.Extensions;

public static class ScopeRequirementExtensions
{
    public static RouteHandlerBuilder RequireScopes(this RouteHandlerBuilder builder, params string[] scopes)
    {
        return builder.RequireAuthorization(policyBuilder =>
        {
            policyBuilder.RequireAuthenticatedUser();
            
            if (scopes.Length > 0)
            {
                policyBuilder.RequireAssertion(context =>
                {
                    var userScopes = context.User.Claims
                        .Where(c => c.Type == "escopos")
                        .Select(c => c.Value)
                        .ToList();

                    return scopes.Any(requiredScope => userScopes.Contains(requiredScope));
                });
            }
        });
    }
}
