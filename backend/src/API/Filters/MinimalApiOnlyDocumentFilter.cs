using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portal.API.Filters;

/// <summary>
/// Filtro para remover controllers tradicionais do Swagger, mantendo apenas Minimal APIs
/// </summary>
public class MinimalApiOnlyDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = new List<string>();

        foreach (var path in swaggerDoc.Paths)
        {
            // Remove paths que correspondem aos controllers tradicionais
            if (IsTraditionalControllerPath(path.Key))
            {
                pathsToRemove.Add(path.Key);
            }
        }

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }

        // Atualizar informações do documento
        swaggerDoc.Info.Title = "Portal SSO - Clean Architecture APIs";
        swaggerDoc.Info.Description = "Apenas endpoints refatorados para Clean Architecture são exibidos. Controllers tradicionais foram ocultados.";
    }

    private static bool IsTraditionalControllerPath(string path)
    {
        // Lista de paths dos controllers tradicionais que queremos remover
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
            "/api/categorias", // Sem versionamento também
            "/api/clientes",
            "/api/equipamentos",
            "/api/locacoes",
            "/api/financeiro",
            "/api/parceiros",
            "/api/perfis", 
            "/api/escopos",
            "/api/auth"
        };

        // Remove paths que começam com estes padrões, EXCETO se contém "v{version}" (Minimal API)
        return traditionalControllerPaths.Any(controllerPath => 
            path.StartsWith(controllerPath, StringComparison.OrdinalIgnoreCase)) &&
            !path.Contains("v{version}", StringComparison.OrdinalIgnoreCase);
    }
}
