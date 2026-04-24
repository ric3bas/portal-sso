using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Portal.API.Endpoints;
using Portal.API.Extensions;
using Portal.API.Filters;
using Portal.Domain.Common;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "PortalCorsPolicy";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");

    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                context.Token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..].Trim()
                    : authHeader.Trim();
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.MapType<Direcao>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = new List<IOpenApiAny>
        {
            new OpenApiString("asc"),
            new OpenApiString("desc")
        },
        Default = new OpenApiString("asc")
    });
});

var app = builder.Build();

app.UseCors(CorsPolicyName);

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

app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html", permanent: false)).AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

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

Console.WriteLine("ðŸš€ Portal SSO API iniciada com sucesso!");
Console.WriteLine("ðŸŒ API: https://localhost:54443");
Console.WriteLine("ðŸ“‹ Swagger: https://localhost:54443/swagger/index.html");
Console.WriteLine("ðŸ·ï¸ Categorias: https://localhost:54443/api/v1/categorias");
Console.WriteLine("ðŸ‘¤ Clientes: https://localhost:54443/api/v1/clientes");
Console.WriteLine("ðŸ§° Equipamentos: https://localhost:54443/api/v1/equipamentos");
Console.WriteLine("ðŸ” Escopos: https://localhost:54443/api/v1/escopos");
Console.WriteLine("ðŸ’° Financeiro: https://localhost:54443/api/v1/financeiro");
Console.WriteLine("ðŸ“¦ LocaÃ§Ãµes: https://localhost:54443/api/v1/locacoes");
Console.WriteLine("ðŸ¤ Parceiros: https://localhost:54443/api/v1/parceiros");
Console.WriteLine("ðŸ§© Perfis: https://localhost:54443/api/v1/perfis");
Console.WriteLine("ðŸ‘¥ UsuÃ¡rios: https://localhost:54443/api/v1/usuarios");
Console.WriteLine("ðŸ”‘ Auth: https://localhost:54443/api/v1/auth");

app.Run();
