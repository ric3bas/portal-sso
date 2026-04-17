using Portal.Features.Categoria.Domain.Interfaces;
using Portal.Features.Categoria.Infra;
using Portal.Features.Categoria.Service;
using Portal.Features.Cliente.Domain.Interfaces;
using Portal.Features.Cliente.Infra;
using Portal.Features.Cliente.Service;
using Portal.Features.Equipamento.Domain.Interfaces;
using Portal.Features.Equipamento.Infra;
using Portal.Features.Equipamento.Service;
using Portal.Features.Financeiro.Domain.Interfaces;
using Portal.Features.Financeiro.Infra;
using Portal.Features.Financeiro.Service;
using Portal.Features.Locacao.Domain.Interfaces;
using Portal.Features.Locacao.Infra;
using Portal.Features.Locacao.Service;

namespace Portal.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocacaoServices(this IServiceCollection services)
        {
            // Categoria
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();

            // Equipamento
            services.AddScoped<IEquipamentoService, EquipamentoService>();
            services.AddScoped<IEquipamentoRepository, EquipamentoRepository>();

            // Cliente
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IClienteRepository, ClienteRepository>();

            // Locacao
            services.AddScoped<ILocacaoService, LocacaoService>();
            services.AddScoped<ILocacaoRepository, LocacaoRepository>();

            // Financeiro
            services.AddScoped<IFinanceiroService, FinanceiroService>();
            services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();

            return services;
        }
    }
}