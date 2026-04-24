using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portal.API.Filters;

public class MinimalApiOnlyDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = new List<string>();

        foreach (var path in swaggerDoc.Paths)
        {
            if (IsTraditionalControllerPath(path.Key))
            {
                pathsToRemove.Add(path.Key);
            }
        }

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }

        swaggerDoc.Info.Title = "Portal SSO - Clean Architecture APIs";
        swaggerDoc.Info.Description = "Apenas endpoints refatorados para Clean Architecture sÃ£o exibidos. Controllers tradicionais foram ocultados.";
    }

    private static bool IsTraditionalControllerPath(string path)
    {
        var traditionalControllerPaths = new[]
        {
            "/api/v1/categorias", // Controller tradicional de categorias
            "/api/v1/clientes",
            "/api/v1/equipamentos", 
            "/api/v1/locacoes",
            "/api/v1/financeiro",
            "/api/v1/parceiros",
            "/api/v1/perfis",
            "/api/v1/escopos",
            "/api/v1/auth",
            "/api/categorias", // Sem versionamento tambÃ©m
            "/api/clientes",
            "/api/equipamentos",
            "/api/locacoes",
            "/api/financeiro",
            "/api/parceiros",
            "/api/perfis", 
            "/api/escopos",
            "/api/auth"
        };

        return traditionalControllerPaths.Any(controllerPath => 
            path.StartsWith(controllerPath, StringComparison.OrdinalIgnoreCase)) &&
            !path.Contains("v{version}", StringComparison.OrdinalIgnoreCase);
    }
}
