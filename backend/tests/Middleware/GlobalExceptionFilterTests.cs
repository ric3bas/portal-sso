using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Portal.Domain.Base;
using Portal.Domain.Exceptions;

namespace sso.global;

public class GlobalExceptionFilterTests
{
    private static ExceptionContext CreateContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-123";
        httpContext.Request.Path = "/api/test";

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    [Fact]
    public void OnException_WithValidationException_ReturnsBadRequestProblem()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionFilter>>();
        var filter = new GlobalExceptionFilter(logger);
        var context = CreateContext(new ValidationException("campo invÃ¡lido"));

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<JsonResult>(context.result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Data.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Erro de validaÃ§Ã£o", problem.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Fact]
    public void OnException_WithNotFoundException_ReturnsNotFoundProblem()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionFilter>>();
        var filter = new GlobalExceptionFilter(logger);
        var context = CreateContext(new NotFoundException("nÃ£o encontrado"));

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<JsonResult>(context.result.Data);
        Assert.Equal(StatusCodes.Status404NotFound, result.Data.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Nenhum registro encontrado", problem.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public void OnException_WithBusinessException_ReturnsUnprocessableEntityProblem()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionFilter>>();
        var filter = new GlobalExceptionFilter(logger);
        var context = CreateContext(new BusinessException("regra de negÃ³cio"));

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<JsonResult>(context.result.Data);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.Data.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problem.Status);
    }

    [Fact]
    public void OnException_WithGenericException_ReturnsInternalServerError()
    {
        var logger = Substitute.For<ILogger<GlobalExceptionFilter>>();
        var filter = new GlobalExceptionFilter(logger);
        var context = CreateContext(new Exception("erro genÃ©rico"));

        filter.OnException(context);

        Assert.True(context.ExceptionHandled);
        var result = Assert.IsType<JsonResult>(context.result.Data);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Data.StatusCode);
        Assert.NotNull(result.Value);
    }
}
