using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Portal.Features.Perfil.Controller;
using Portal.Features.Perfil.Domain;
using Portal.Features.Perfil.Domain.Interfaces;

namespace sso.controllers;

public class PerfisControllerTests
{
    [Fact]
    public void Constructor_SetsPerfilService()
    {
        var service = Substitute.For<IPerfilService>();

        var controller = new PerfisController(service);

        Assert.NotNull(controller);
    }

    [Fact]
    public async Task GetAllComEscoposAsync_ReturnsResultFromService()
    {
        var service = Substitute.For<IPerfilService>();
        var expectedResult = new List<PerfilComEscopoResponse>
        {
            new() { Id = 1, Nome = "Admin", Escopos = [] },
            new() { Id = 2, Nome = "User", Escopos = [] }
        };
        service.ListarComEscoposAsync(Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<IEnumerable<PerfilComEscopoResponse>>(expectedresult.Data));

        var controller = new PerfisController(service);
        var cancellationToken = CancellationToken.None;

        var result = await controller.GetAllComEscoposAsync(cancellationToken);

        Assert.Same(expectedresult.Data, result.Data);
    }

    [Fact]
    public async Task GetAllComEscoposAsync_CallsServiceListarComEscoposAsync()
    {
        var service = Substitute.For<IPerfilService>();
        var controller = new PerfisController(service);
        var cancellationToken = CancellationToken.None;

        await controller.GetAllComEscoposAsync(cancellationToken);

        await service.Received(1).ListarComEscoposAsync(cancellationToken);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOkWithPerfil()
    {
        var service = Substitute.For<IPerfilService>();
        var expectedPerfil = new PerfilResponse { Id = 1, Nome = "Admin" };
        service.ObterPorIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult<PerfilResponse?>(expectedPerfil));

        var controller = new PerfisController(service);
        var id = 1;
        var cancellationToken = CancellationToken.None;

        var result = await controller.GetByIdAsync(id, cancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result.Data);
        Assert.Equal(200, okresult.Data.StatusCode);
        Assert.Same(expectedPerfil, okResult.Value);
    }

    [Fact]
    public async Task GetByIdAsync_CallsServiceObterPorIdAsync()
    {
        var service = Substitute.For<IPerfilService>();
        var controller = new PerfisController(service);
        var id = 1;
        var cancellationToken = CancellationToken.None;

        await controller.GetByIdAsync(id, cancellationToken);

        await service.Received(1).ObterPorIdAsync(id, cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_Returns201WithId()
    {
        var service = Substitute.For<IPerfilService>();
        var expectedId = 42;
        service.CriarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
               .Returns(Task.FromResult(expectedId));

        var controller = new PerfisController(service);
        var request = new PerfilRequest { Nome = "NewPerfil" };
        var cancellationToken = CancellationToken.None;

        var result = await controller.CreateAsync(request, cancellationToken);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Data);
        Assert.Equal(201, statusCoderesult.Data.StatusCode);
        
        var value = statusCodeResult.Value;
        Assert.NotNull(value);
        var idProperty = value.GetType().GetProperty("id");
        Assert.NotNull(idProperty);
        Assert.Equal(expectedId, idProperty.GetValue(value));
    }

    [Fact]
    public async Task CreateAsync_CallsServiceCriarAsync()
    {
        var service = Substitute.For<IPerfilService>();
        var controller = new PerfisController(service);
        var request = new PerfilRequest { Nome = "NewPerfil" };
        var cancellationToken = CancellationToken.None;

        await controller.CreateAsync(request, cancellationToken);

        await service.Received(1).CriarAsync(request.Nome, cancellationToken);
    }

    [Fact]
    public async Task VincularEscoposAsync_ReturnsNoContent()
    {
        var service = Substitute.For<IPerfilService>();
        service.VincularEscoposAsync(Arg.Any<int>(), Arg.Any<int[]>(), Arg.Any<CancellationToken>())
               .Returns(Task.CompletedTask);

        var controller = new PerfisController(service);
        var id = 1;
        var request = new VincularEscopoRequest { EscopoIds = [1, 2, 3] };
        var cancellationToken = CancellationToken.None;

        var result = await controller.VincularEscoposAsync(id, request, cancellationToken);

        var noContentResult = Assert.IsType<NoContentResult>(result.Data);
        Assert.Equal(204, noContentresult.Data.StatusCode);
    }

    [Fact]
    public async Task VincularEscoposAsync_CallsServiceVincularEscoposAsync()
    {
        var service = Substitute.For<IPerfilService>();
        var controller = new PerfisController(service);
        var id = 1;
        var request = new VincularEscopoRequest { EscopoIds = [1, 2, 3] };
        var cancellationToken = CancellationToken.None;

        await controller.VincularEscoposAsync(id, request, cancellationToken);

        await service.Received(1).VincularEscoposAsync(id, request.EscopoIds, cancellationToken);
    }
}
