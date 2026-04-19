using Microsoft.OpenApi.Models;
using Npgsql;
using Portal.API.Endpoints;
using Portal.API.Extensions;
using Portal.API.Filters;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "PortalCorsPolicy";

// Configuração explícita do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddScoped<IDbConnection>(_ =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCleanArchitecture();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0.0",
        Title = "Portal SSO API",
        Description = "API do Portal SSO com Minimal APIs",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Portal SSO Support",
            Email = "support@portal.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    options.SchemaFilter<FluentValidationSchemaFilter>();
});

var app = builder.Build();

app.UseCors(CorsPolicyName);

// Configurar Swagger UI
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portal SSO API v1.0.0");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Portal SSO API Documentation";
});

app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html", permanent: false));

// Minimal APIs - Categoria
app.MapCategoriaEndpoints();
app.MapClienteEndpoints();
app.MapEquipamentoEndpoints();
app.MapEscopoEndpoints();
app.MapFinanceiroEndpoints();
app.MapLocacaoEndpoints();
app.MapParceiroEndpoints();
app.MapPerfilEndpoints();
app.MapUsuarioEndpoints();
app.MapAuthEndpoints();

Console.WriteLine("🚀 Portal SSO API iniciada com sucesso!");
Console.WriteLine("🌐 API: https://localhost:54443");
Console.WriteLine("📋 Swagger: https://localhost:54443/swagger/index.html");
Console.WriteLine("🏷️ Categorias: https://localhost:54443/api/v1/categorias");
Console.WriteLine("👤 Clientes: https://localhost:54443/api/v1/clientes");
Console.WriteLine("🧰 Equipamentos: https://localhost:54443/api/v1/equipamentos");
Console.WriteLine("🔐 Escopos: https://localhost:54443/api/v1/escopos");
Console.WriteLine("💰 Financeiro: https://localhost:54443/api/v1/financeiro");
Console.WriteLine("📦 Locações: https://localhost:54443/api/v1/locacoes");
Console.WriteLine("🤝 Parceiros: https://localhost:54443/api/v1/parceiros");
Console.WriteLine("🧩 Perfis: https://localhost:54443/api/v1/perfis");
Console.WriteLine("👥 Usuários: https://localhost:54443/api/v1/usuarios");
Console.WriteLine("🔑 Auth: https://localhost:54443/api/v1/auth");

app.Run();
