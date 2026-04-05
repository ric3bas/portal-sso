using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Portal.Domain.Base;
using Portal.Domain.Base.Email;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Infra;
using Portal.Features.Usuario.Service;
using Portal.Infra;
using Serilog;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

[ExcludeFromCodeCoverage]

string TranslateValidationMessage(string message, string key) {
    if (string.IsNullOrWhiteSpace(message))
        return "Erro de validação.";

    if (message.Contains("field is required", StringComparison.OrdinalIgnoreCase)) {
        var marker = " field is required";
        var start = message.StartsWith("The ", StringComparison.OrdinalIgnoreCase) ? 4 : 0;
        var end = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        var field = end > start ? message[start..end].Trim() : GetFieldNameFromKey(key);
        return $"O campo {field} é obrigatório";
    }

    if (message.Contains("A non-empty request body is required", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("is invalid after a value", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("Expected either", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("BytePositionInLine", StringComparison.OrdinalIgnoreCase))
        return "Json está em um formato incorreto";

    return message;
}

[ExcludeFromCodeCoverage]

string GetFieldNameFromKey(string key) {
    if (string.IsNullOrWhiteSpace(key) || key == "$")
        return "informado";

    var normalized = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
    var separatorIndex = normalized.LastIndexOf('.');
    return separatorIndex >= 0 ? normalized[(separatorIndex + 1)..] : normalized;
}

[ExcludeFromCodeCoverage]

bool IsBodyParameterKey(ActionContext context, string key) {
    if (string.IsNullOrWhiteSpace(key) || key == "$" || key.Contains('.'))
        return false;

    if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        return false;

    return descriptor.Parameters
        .OfType<ControllerParameterDescriptor>()
        .Any(parameter =>
            string.Equals(parameter.Name, key, StringComparison.OrdinalIgnoreCase) &&
            parameter.BindingInfo?.BindingSource == BindingSource.Body);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
});



    // Custom policy para TenantId (ParceiroId)
    builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, TenantIdHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TenantIdPolicy", policy =>
        policy.Requirements.Add(new Portal.Domain.Base.TenantIdRequirement()));
});
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Portal.Infra.DapperDatabaseProvider>();
builder.Services.AddScoped<Portal.Infra.IUnitOfWork>(sp =>
{
    var provider = sp.GetRequiredService<Portal.Infra.DapperDatabaseProvider>();
    // Use o contexto padrão ou ajuste conforme necessário
    var connection = provider.CreateConnection("SSO_POSTGRES");
    return new Portal.Infra.UnitOfWork(connection);
});
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<Portal.Features.Perfil.Domain.Interfaces.IPerfilRepository, Portal.Features.Perfil.Infra.PerfilRepository>();
builder.Services.AddScoped<Portal.Features.Perfil.Domain.Interfaces.IPerfilService, Portal.Features.Perfil.Service.PerfilService>();
builder.Services.AddScoped<Portal.Features.Escopo.Domain.Interfaces.IEscopoRepository, Portal.Features.Escopo.Infra.EscopoRepository>();
builder.Services.AddScoped<Portal.Features.Escopo.Domain.Interfaces.IEscopoService, Portal.Features.Escopo.Service.EscopoService>();

builder.Services.AddScoped<ITokenAtualizacaoRepository, TokenAtualizacaoRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddControllers(options => {
    options.Filters.Add<GlobalExceptionFilter>();
}).ConfigureApiBehaviorOptions(options => {
    options.InvalidModelStateResponseFactory = context => {
        // Verificar se há erro de JSON inválido
        var hasJsonError = context.ModelState.Values.Any(v =>
            v.Errors.Any(e =>
                e.ErrorMessage.Contains("is invalid after a value", StringComparison.OrdinalIgnoreCase) ||
                e.ErrorMessage.Contains("Expected either", StringComparison.OrdinalIgnoreCase) ||
                e.ErrorMessage.Contains("BytePositionInLine", StringComparison.OrdinalIgnoreCase) ||
                e.ErrorMessage.Contains("The JSON value could not be converted", StringComparison.OrdinalIgnoreCase) ||
                e.ErrorMessage.Contains("A non-empty request body is required", StringComparison.OrdinalIgnoreCase)));

        string[] errors;

        if (hasJsonError) {
            errors = new[] { "Json está em um formato incorreto" };
        } else {
            errors = context.ModelState
                .Where(entry => entry.Value is { Errors.Count: > 0 })
                .SelectMany(entry => entry.Value!.Errors.Select(error => {
                    var translatedMessage = TranslateValidationMessage(error.ErrorMessage, entry.Key);
                    var fieldName = GetFieldNameFromKey(entry.Key);
                    var isBodyParameter = IsBodyParameterKey(context, entry.Key);

                    if (string.IsNullOrWhiteSpace(entry.Key) || entry.Key == "$" || isBodyParameter)
                        return translatedMessage;

                    if (translatedMessage.Contains($"campo {fieldName}", StringComparison.OrdinalIgnoreCase))
                        return translatedMessage;

                    return $"{fieldName}: {translatedMessage}";
                }))
                .ToArray();
        }

        var problem = new ProblemDetails {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "Erro de validação",
            Status = StatusCodes.Status400BadRequest,
            Detail = "A requisição contém erros de validação.",
            Instance = context.HttpContext.Request.Path,
        };

        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        problem.Extensions["errors"] = errors;

        return new BadRequestObjectResult(problem);
    };
});
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new UrlSegmentApiVersionReader()
    );
});
builder.Services.AddVersionedApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.EnableAnnotations();
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Portal SSO",
        Version = "v1",
        Description = "API de autenticação e autorização para o Portal",
        Contact = new OpenApiContact {
            Name = "Equipe de Desenvolvimento",
            Email = "ric3bas@gmail.com"
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddFluentValidationRulesToSwagger(schemaGenerationOptions => {
    schemaGenerationOptions.SetNotNullableIfMinLengthGreaterThenZero = true;
    schemaGenerationOptions.SetNotNullableIfMinimumGreaterThenZero = true;
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressMapClientErrors = true;
});

    builder.Services.AddAuthorization();

builder.Services.AddSingleton<DapperDatabaseProvider>();

builder.Services.AddScoped<Portal.Features.Parceiro.Domain.Interfaces.IParceiroRepository, Portal.Features.Parceiro.Infra.ParceiroRepository>();
builder.Services.AddScoped<Portal.Features.Parceiro.Domain.Interfaces.IParceiroService, Portal.Features.Parceiro.Service.ParceiroService>();


var app = builder.Build();

var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();


if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Portal API {description.GroupName.ToUpperInvariant()}");
        }
        c.RoutePrefix = "swagger";
    });
}
app.UseCors("AllowAll");

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<JwtRevocationMiddleware>();

app.MapControllers(); // Mantém para compatibilidade
app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

[ExcludeFromCodeCoverage]
public partial class Program;

