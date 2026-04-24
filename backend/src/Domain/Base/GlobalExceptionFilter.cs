using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Portal.Domain.Exceptions;

namespace Portal.Domain.Base
{
    public class GlobalExceptionFilter : IExceptionFilter {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) {
            _logger = logger;
        }

        public void OnException(ExceptionContext context) {
            switch (context.Exception)
            {
                case ValidationException ex:
                    _logger.LogWarning(ex, "Erro de validacao em {RequestPath}", context.HttpContext.Request.Path);
                    var problemBadRequest = new ProblemDetails
                    {
                        Title = "Erro de validaÃ§Ã£o",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "A requisiÃ§Ã£o contÃ©m erros de validaÃ§Ã£o.",
                        Instance = context.HttpContext.Request.Path,
                    };
                    problemBadRequest.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    problemBadRequest.Extensions["errors"] = ex.Errors.ToArray();

                    context.Result = new JsonResult(problemBadRequest) { StatusCode = StatusCodes.Status400BadRequest };
                    context.ExceptionHandled = true;
                    break;
                case NotFoundException ex:
                    _logger.LogWarning(ex, "Recurso nao encontrado em {RequestPath}", context.HttpContext.Request.Path);
                    var problem = new ProblemDetails
                    {
                        Title = "Nenhum registro encontrado",
                        Status = StatusCodes.Status404NotFound,
                        Detail = ex.Message,
                        Instance = context.HttpContext.Request.Path,
                    };
                    problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                    context.Result = new JsonResult(problem) { StatusCode = StatusCodes.Status404NotFound };
                    context.ExceptionHandled = true;
                    break;
                case BusinessException ex:
                    _logger.LogWarning(ex, "Erro de negÃ³cio em {RequestPath}", context.HttpContext.Request.Path);
                    var problemBusiness = new ProblemDetails
                    {
                        Title = "Nenhum registro encontrado",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = ex.Message,
                        Instance = context.HttpContext.Request.Path,
                    };
                    problemBusiness.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    problemBusiness.Extensions["errors"] = ex.Errors.ToArray();

                    context.Result = new JsonResult(problemBusiness) { StatusCode = StatusCodes.Status422UnprocessableEntity };
                    context.ExceptionHandled = true;
                    break;
                default:
                    _logger.LogError(context.Exception, "Erro nao tratado em {RequestPath}", context.HttpContext.Request.Path);
                    var genericProblem = new {
                        type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                        title = "Erro interno do servidor.",
                        status = 500,
                        detail = context.Exception.Message,
                        traceId = context.HttpContext.TraceIdentifier
                    };
                    context.Result = new JsonResult(genericProblem) { StatusCode = 500 };
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}
