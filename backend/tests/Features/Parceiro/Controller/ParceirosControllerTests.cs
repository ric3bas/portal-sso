using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Portal.Features.Parceiro.Controller;
using Portal.Features.Parceiro.Domain;
using Portal.Features.Parceiro.Domain.Interfaces;

namespace sso.controllers;

public class ParceirosControllerTests
{
    private static readonly Type _controller = typeof(ParceirosController);

    [Theory]
    [InlineData("GetAllAsync",  typeof(HttpGetAttribute))]
    [InlineData("GetByIdAsync", typeof(HttpGetAttribute))]
    [InlineData("CreateAsync",  typeof(HttpPostAttribute))]
    [InlineData("UpdateAsync",  typeof(HttpPutAttribute))]
    public void Endpoint_Exists(string methodName, Type httpAttribute)
    {
        var method = _controller.GetMethod(methodName);

        Assert.NotNull(method);
        Assert.True(method.GetCustomAttributes(httpAttribute, false).Length > 0);
    }

    [Fact]
    public async Task GetAllAsync_Returns200()
    {
        var service = Substitute.For<IParceiroService>();
        service.ListarParceirosAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
               .Returns([new ParceiroResponse { Id = Guid.NewGuid(), Nome = "Teste", Ativo = true }]);

        var controller = new ParceirosController(service);

        var result = await controller.GetAllAsync(null, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_Returns200()
    {
        var service = Substitute.For<IParceiroService>();
        var parceiro = new ParceiroResponse { Id = Guid.NewGuid(), Nome = "Teste", Ativo = true };
        service.ObterParceiroAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
               .Returns(parceiro);

        var controller = new ParceirosController(service);

        var result = await controller.GetByIdAsync(parceiro.Id.ToString(), CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Equal(parceiro.Id, result!.Id);
    }

    [Fact]
    public async Task CreateAsync_Returns201()
    {
        var service = Substitute.For<IParceiroService>();
        var createdId = Guid.NewGuid();
        service.CriarParceiroAsync(Arg.Any<ParceiroRequest>(), Arg.Any<CancellationToken>())
               .Returns(createdId);

        var controller = new ParceirosController(service);

        var result = await controller.CreateAsync(new ParceiroRequest { Nome = "Novo" }, CancellationToken.None);

        Assert.Equal(createdId, result.Data);
    }

    [Fact]
    public async Task UpdateAsync_Returns204()
    {
        var service = Substitute.For<IParceiroService>();
        service.AtualizarParceiroAsync(Arg.Any<AtualizarParceiroRequest>(), Arg.Any<CancellationToken>())
               .Returns(Task.CompletedTask);

        var controller = new ParceirosController(service);

        var result = await controller.UpdateAsync("123", new AtualizarParceiroRequest { Nome = "Atualizado", Ativo = true }, CancellationToken.None);

        var noContent = Assert.IsType<NoContentResult>(result.Data);
        Assert.Equal(204, noContent.StatusCode);
    }
}
