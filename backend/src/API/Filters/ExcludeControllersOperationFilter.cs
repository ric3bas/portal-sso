using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portal.API.Filters;

public class ExcludeControllersOperationFilter : IOperationFilter
{
    private readonly HashSet<string> _excludedControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "CategoriasController",
        "ClientesController", 
        "EquipamentosController",
        "LocacoesController",
        "FinanceiroController",
        "ParceirosController",
        "PerfisController",
        "EscoposController",
        "AuthController"
    };

    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
        {
            if (_excludedControllers.Contains(controllerDescriptor.ControllerName))
            {
                operation.Tags?.Clear();
                operation.Summary = null;
                operation.Description = null;
            }
        }
    }
}
