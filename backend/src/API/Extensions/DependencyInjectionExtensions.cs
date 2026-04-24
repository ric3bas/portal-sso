using Portal.Application.Cliente.UseCases.AtualizarCliente;
using Portal.Application.Cliente.UseCases.BloquearCliente;
using Portal.Application.Cliente.UseCases.CriarCliente;
using Portal.Application.Cliente.UseCases.DesbloquearCliente;
using Portal.Application.Cliente.UseCases.InativarCliente;
using Portal.Application.Cliente.UseCases.ObterClientePorId;
using Portal.Application.Cliente.UseCases.ObterClientes;
using Portal.Application.Cliente.UseCases.ObterClientesPorFiltro;
using Portal.Application.Equipamento.UseCases.AtualizarEquipamento;
using Portal.Application.Equipamento.UseCases.CriarEquipamento;
using Portal.Application.Equipamento.UseCases.InativarEquipamento;
using Portal.Application.Equipamento.UseCases.ObterEquipamentoPorId;
using Portal.Application.Equipamento.UseCases.ObterEquipamentos;
using Portal.Application.Equipamento.UseCases.ObterEquipamentosPorFiltro;
using Portal.Application.Escopo.UseCases.AtualizarEscopo;
using Portal.Application.Escopo.UseCases.CriarEscopo;
using Portal.Application.Escopo.UseCases.ObterEscopoPorId;
using Portal.Application.Escopo.UseCases.ObterEscopos;
using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceiros;
using Portal.Application.Financeiro.UseCases.ObterLancamentosFinanceirosPorPeriodo;
using Portal.Application.Locacao.UseCases.AtualizarLocacao;
using Portal.Application.Locacao.UseCases.CancelarLocacao;
using Portal.Application.Locacao.UseCases.CriarLocacao;
using Portal.Application.Locacao.UseCases.DevolverLocacao;
using Portal.Application.Locacao.UseCases.ObterLocacaoPorId;
using Portal.Application.Locacao.UseCases.ObterLocacoes;
using Portal.Application.Locacao.UseCases.ObterLocacoesAtrasadas;
using Portal.Application.Locacao.UseCases.ObterLocacoesPorFiltro;
using Portal.Application.Parceiro.UseCases.AtualizarParceiro;
using Portal.Application.Parceiro.UseCases.CriarParceiro;
using Portal.Application.Parceiro.UseCases.ObterParceiroPorId;
using Portal.Application.Parceiro.UseCases.ObterParceiros;
using Portal.Application.Parceiro.UseCases.ObterParceirosPorFiltro;
using Portal.Application.Perfil.UseCases.ApagarPerfil;
using Portal.Application.Perfil.UseCases.AtualizarNomePerfil;
using Portal.Application.Perfil.UseCases.ClonarPerfil;
using Portal.Application.Perfil.UseCases.CriarPerfil;
using Portal.Application.Perfil.UseCases.ObterPerfilPorId;
using Portal.Application.Perfil.UseCases.ObterPerfisComEscopo;
using Portal.Application.Perfil.UseCases.ObterPerfisParaCombo;
using Portal.Application.Perfil.UseCases.VincularEscoposPerfil;
using Portal.Application.Auth.UseCases.Login;
using Portal.Application.Auth.UseCases.Logout;
using Portal.Application.Auth.UseCases.RecuperarSenha;
using Portal.Application.Auth.UseCases.RefreshToken;
using Portal.Application.Auth.UseCases.TrocarSenha;
using Portal.Application.Categoria.UseCases.CriarCategoria;
using Portal.Application.Categoria.UseCases.ObterCategorias;
using Portal.Application.Categoria.UseCases.ObterCategoriaPorId;
using Portal.Application.Categoria.UseCases.AtualizarCategoria;
using Portal.Application.Categoria.UseCases.InativarCategoria;
using Portal.Application.Categoria.UseCases.ObterCategoriasPorFiltro;
using Portal.Application.Usuario.UseCases.AtualizarUsuario;
using Portal.Application.Usuario.UseCases.ObterUsuarios;
using Portal.Application.Usuario.UseCases.CriarUsuario;
using Portal.Domain.Base.Email;
using Portal.Domain.Cliente.Interfaces;
using Portal.Domain.Categoria.Interfaces;
using Portal.Domain.Equipamento.Interfaces;
using Portal.Domain.Escopo.Interfaces;
using Portal.Domain.Financeiro.Interfaces;
using Portal.Domain.Locacao.Interfaces;
using Portal.Domain.Parceiro.Interfaces;
using Portal.Domain.Perfil.Interfaces;
using Portal.Domain.Usuario.Interfaces;
using Portal.Infrastructure.Data;
using Portal.Infrastructure.Repositories;

namespace Portal.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCleanArchitecture(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();
        services.AddScoped<IEscopoRepository, EscopoRepository>();
        services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
        services.AddScoped<ILocacaoRepository, LocacaoRepository>();
        services.AddScoped<IParceiroRepository, ParceiroRepository>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITokenAtualizacaoRepository, TokenAtualizacaoRepository>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        
        services.AddScoped<CriarCategoriaHandler>();
        services.AddScoped<ObterCategoriasHandler>();
        services.AddScoped<ObterCategoriaPorIdHandler>();
        services.AddScoped<AtualizarCategoriaHandler>();
        services.AddScoped<InativarCategoriaHandler>();
        services.AddScoped<ObterCategoriasPorFiltroHandler>();

        services.AddScoped<ObterClientesHandler>();
        services.AddScoped<ObterClientesPorFiltroHandler>();
        services.AddScoped<ObterClientePorIdHandler>();
        services.AddScoped<CriarClienteHandler>();
        services.AddScoped<AtualizarClienteHandler>();
        services.AddScoped<BloquearClienteHandler>();
        services.AddScoped<DesbloquearClienteHandler>();
        services.AddScoped<InativarClienteHandler>();

        services.AddScoped<ObterEquipamentosHandler>();
        services.AddScoped<ObterEquipamentosPorFiltroHandler>();
        services.AddScoped<ObterEquipamentoPorIdHandler>();
        services.AddScoped<CriarEquipamentoHandler>();
        services.AddScoped<AtualizarEquipamentoHandler>();
        services.AddScoped<InativarEquipamentoHandler>();

        services.AddScoped<ObterEscoposHandler>();
        services.AddScoped<ObterEscopoPorIdHandler>();
        services.AddScoped<CriarEscopoHandler>();
        services.AddScoped<AtualizarEscopoHandler>();

        services.AddScoped<ObterLancamentosFinanceirosHandler>();
        services.AddScoped<ObterLancamentosFinanceirosPorPeriodoHandler>();

        services.AddScoped<ObterLocacoesHandler>();
        services.AddScoped<ObterLocacoesPorFiltroHandler>();
        services.AddScoped<ObterLocacaoPorIdHandler>();
        services.AddScoped<CriarLocacaoHandler>();
        services.AddScoped<AtualizarLocacaoHandler>();
        services.AddScoped<DevolverLocacaoHandler>();
        services.AddScoped<CancelarLocacaoHandler>();
        services.AddScoped<ObterLocacoesAtrasadasHandler>();

        services.AddScoped<ObterParceirosHandler>();
        services.AddScoped<ObterParceirosPorFiltroHandler>();
        services.AddScoped<ObterParceiroPorIdHandler>();
        services.AddScoped<CriarParceiroHandler>();
        services.AddScoped<AtualizarParceiroHandler>();

        services.AddScoped<ObterPerfisComEscopoHandler>();
        services.AddScoped<ObterPerfisParaComboHandler>();
        services.AddScoped<ObterPerfilPorIdHandler>();
        services.AddScoped<CriarPerfilHandler>();
        services.AddScoped<VincularEscoposPerfilHandler>();
        services.AddScoped<ApagarPerfilHandler>();
        services.AddScoped<ClonarPerfilHandler>();
        services.AddScoped<AtualizarNomePerfilHandler>();

        services.AddScoped<ObterUsuariosHandler>();
        services.AddScoped<CriarUsuarioHandler>();
        services.AddScoped<AtualizarUsuarioHandler>();

        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<RecuperarSenhaHandler>();
        services.AddScoped<ValidarTokenRecuperacaoHandler>();
        
        return services;
    }
}

