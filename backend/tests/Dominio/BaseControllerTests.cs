using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.Domain.Base;

namespace sso.global;

public class BaseControllerTests
{
    private sealed class TestableBaseController : BaseController
    {
        public new ObjectResult BadRequestProblem(string detail, string title = "Erro de validação")
            => base.BadRequestProblem(detail, title);

        public new ObjectResult NotFoundProblem(string detail, string title = "Nenhum registro encontrado")
            => base.NotFoundProblem(detail, title);

        public new ObjectResult InternalServerErrorProblem(string detail, string title = "Erro interno do servidor")
            => base.InternalServerErrorProblem(detail, title);

        public new ObjectResult BusinessErrorProblem(string detail, string title = "Erro de negócio")
            => base.BusinessErrorProblem(detail, title);
    }

    [Fact]
    public void BadRequestProblem_WithDetailAndTitle_ReturnsObjectResultWithCorrectProperties()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.BadRequestProblem("Test detail", "Test title");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Test detail", problemDetails.Detail);
        Assert.Equal("Test title", problemDetails.Title);
        Assert.Equal("/test/path", problemDetails.Instance);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }

    [Fact]
    public void BadRequestProblem_WithDetailOnly_UsesDefaultTitle()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.BadRequestProblem("Validation error");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Validation error", problemDetails.Detail);
        Assert.Equal("Erro de validação", problemDetails.Title);
    }

    [Fact]
    public void BadRequestProblem_WithNullHttpContext_ReturnsResultWithNullInstance()
    {
        var controller = new TestableBaseController();

        var result = controller.BadRequestProblem("Error detail", "Error title");

        Assert.NotNull(result.Data);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Null(problemDetails.Instance);
    }

    [Fact]
    public void NotFoundProblem_WithDetailAndTitle_ReturnsObjectResultWithCorrectProperties()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.NotFoundProblem("Resource not found", "Custom not found");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status404NotFound, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Resource not found", problemDetails.Detail);
        Assert.Equal("Custom not found", problemDetails.Title);
        Assert.Equal("/test/path", problemDetails.Instance);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public void NotFoundProblem_WithDetailOnly_UsesDefaultTitle()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/resource";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.NotFoundProblem("Item not found");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status404NotFound, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Item not found", problemDetails.Detail);
        Assert.Equal("Nenhum registro encontrado", problemDetails.Title);
    }

    [Fact]
    public void NotFoundProblem_WithNullHttpContext_ReturnsResultWithNullInstance()
    {
        var controller = new TestableBaseController();

        var result = controller.NotFoundProblem("Not found");

        Assert.NotNull(result.Data);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Null(problemDetails.Instance);
    }

    [Fact]
    public void InternalServerErrorProblem_WithDetailAndTitle_ReturnsObjectResultWithCorrectProperties()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.InternalServerErrorProblem("Server error", "Custom error");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Server error", problemDetails.Detail);
        Assert.Equal("Custom error", problemDetails.Title);
        Assert.Equal("/test/path", problemDetails.Instance);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
    }

    [Fact]
    public void InternalServerErrorProblem_WithDetailOnly_UsesDefaultTitle()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/error";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.InternalServerErrorProblem("Something went wrong");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Something went wrong", problemDetails.Detail);
        Assert.Equal("Erro interno do servidor", problemDetails.Title);
    }

    [Fact]
    public void InternalServerErrorProblem_WithNullHttpContext_ReturnsResultWithNullInstance()
    {
        var controller = new TestableBaseController();

        var result = controller.InternalServerErrorProblem("Error");

        Assert.NotNull(result.Data);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Null(problemDetails.Instance);
    }

    [Fact]
    public void BusinessErrorProblem_WithDetailAndTitle_ReturnsObjectResultWithCorrectProperties()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.BusinessErrorProblem("Business rule violation", "Custom business error");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Business rule violation", problemDetails.Detail);
        Assert.Equal("Custom business error", problemDetails.Title);
        Assert.Equal("/test/path", problemDetails.Instance);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.Status);
    }

    [Fact]
    public void BusinessErrorProblem_WithDetailOnly_UsesDefaultTitle()
    {
        var controller = new TestableBaseController();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/business";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = controller.BusinessErrorProblem("Rule failed");

        Assert.NotNull(result.Data);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.Data.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Rule failed", problemDetails.Detail);
        Assert.Equal("Erro de negócio", problemDetails.Title);
    }

    [Fact]
    public void BusinessErrorProblem_WithNullHttpContext_ReturnsResultWithNullInstance()
    {
        var controller = new TestableBaseController();

        var result = controller.BusinessErrorProblem("Business error");

        Assert.NotNull(result.Data);
        var problemDetails = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Null(problemDetails.Instance);
    }

    [Fact]
    public void ProducesBadRequestProblemAttribute_Constructor_SetsBaseTypeAndStatusCode()
    {
        var attribute = new ProducesBadRequestProblemAttribute();

        Assert.NotNull(attribute);
        Assert.Equal(typeof(ProblemDetails), attribute.Type);
        Assert.Equal(StatusCodes.Status400BadRequest, attribute.StatusCode);
    }

    [Fact]
    public void ProducesNotFoundProblemAttribute_Constructor_SetsBaseTypeAndStatusCode()
    {
        var attribute = new ProducesNotFoundProblemAttribute();

        Assert.NotNull(attribute);
        Assert.Equal(typeof(ProblemDetails), attribute.Type);
        Assert.Equal(StatusCodes.Status404NotFound, attribute.StatusCode);
    }

    [Fact]
    public void ProducesBusinessProblemAttribute_Constructor_SetsBaseTypeAndStatusCode()
    {
        var attribute = new ProducesBusinessProblemAttribute();

        Assert.NotNull(attribute);
        Assert.Equal(typeof(ProblemDetails), attribute.Type);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, attribute.StatusCode);
    }
}
