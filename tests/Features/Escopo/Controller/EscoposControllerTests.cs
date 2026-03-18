using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Portal.Features.Escopo.Controller;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using EscopoEntity = Portal.Dominio.Entities.Escopo;

namespace sso.controllers;

public class EscoposControllerTests
{
    private static readonly Type _controller = typeof(EscoposController);

    [Fact]
    public void Constructor_ShouldSetService()
    {
        var service = Substitute.For<IEscopoService>();

        var controller = new EscoposController(service);

        Assert.NotNull(controller);
    }

    [Theory]
    [InlineData("GetAllAsync", typeof(HttpGetAttribute))]
    [InlineData("GetByIdAsync", typeof(HttpGetAttribute))]
    [InlineData("CreateAsync", typeof(HttpPostAttribute))]
    public void Endpoint_Exists(string methodName, Type httpAttribute)
    {
        var method = _controller.GetMethod(methodName);

        Assert.NotNull(method);
        Assert.True(method.GetCustomAttributes(httpAttribute, false).Length > 0);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOkResult()
    {
        var service = Substitute.For<IEscopoService>();
        var escopos = new List<EscopoResponse>
        {
            new EscopoResponse { Id = 1, Nome = "Escopo1" },
            new EscopoResponse { Id = 2, Nome = "Escopo2" }
        };
        service.ListarAsync(Arg.Any<CancellationToken>())
               .Returns(escopos);

        var controller = new EscoposController(service);

        var result = await controller.GetAllAsync(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(escopos, okResult.Value);
    }

    [Fact]
    public async Task GetAllAsync_CallsServiceWithCancellationToken()
    {
        var service = Substitute.For<IEscopoService>();
        var cancellationToken = new CancellationToken();
        service.ListarAsync(Arg.Any<CancellationToken>())
               .Returns(new List<EscopoResponse>());

        var controller = new EscoposController(service);

        await controller.GetAllAsync(cancellationToken);

        await service.Received(1).ListarAsync(cancellationToken);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOkResult()
    {
        var service = Substitute.For<IEscopoService>();
        var escopo = new EscopoEntity { Id = 1, Nome = "Escopo Teste" };
        service.ObterPorIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(escopo);

        var controller = new EscoposController(service);

        var result = await controller.GetByIdAsync(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(escopo, okResult.Value);
    }

    [Fact]
    public async Task GetByIdAsync_CallsServiceWithCorrectParameters()
    {
        var service = Substitute.For<IEscopoService>();
        var cancellationToken = new CancellationToken();
        var escopoId = 42;
        service.ObterPorIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(new EscopoEntity { Id = escopoId, Nome = "Teste" });

        var controller = new EscoposController(service);

        await controller.GetByIdAsync(escopoId, cancellationToken);

        await service.Received(1).ObterPorIdAsync(escopoId, cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_Returns201WithId()
    {
        var service = Substitute.For<IEscopoService>();
        var createdId = 123;
        service.CriarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(createdId);

        var controller = new EscoposController(service);
        var request = new EscopoRequest { Nome = "Novo Escopo" };

        var result = await controller.CreateAsync(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        
        var value = objectResult.Value;
        Assert.NotNull(value);
        
        var idProperty = value!.GetType().GetProperty("id");
        Assert.NotNull(idProperty);
        Assert.Equal(createdId, idProperty!.GetValue(value));
    }

    [Fact]
    public async Task CreateAsync_CallsServiceWithCorrectParameters()
    {
        var service = Substitute.For<IEscopoService>();
        var cancellationToken = new CancellationToken();
        service.CriarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(1);

        var controller = new EscoposController(service);
        var request = new EscopoRequest { Nome = "Escopo Teste" };

        await controller.CreateAsync(request, cancellationToken);

        await service.Received(1).CriarAsync(request.Nome, cancellationToken);
    }
}
