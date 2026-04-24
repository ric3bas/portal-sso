using FluentValidation;
using FluentValidation.Validators;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Portal.API.Filters;

public class FluentValidationSchemaFilter : ISchemaFilter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FluentValidationSchemaFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties.Count == 0)
        {
            return;
        }

        var validatorType = typeof(IValidator<>).MakeGenericType(context.Type);
        using var scope = _scopeFactory.CreateScope();
        var validator = scope.ServiceProvider.GetService(validatorType) as IValidator;

        if (validator is null)
        {
            return;
        }

        var descriptor = validator.CreateDescriptor();
        schema.Required ??= new HashSet<string>();

        foreach (var memberGroup in descriptor.GetMembersWithValidators())
        {
            var memberName = memberGroup.Key;
            var propertyName = schema.Properties.Keys.FirstOrDefault(k =>
                string.Equals(k, memberName, StringComparison.OrdinalIgnoreCase));

            if (propertyName is null)
            {
                propertyName = schema.Properties.Keys.FirstOrDefault(k =>
                    string.Equals(k, ToCamelCase(memberName), StringComparison.OrdinalIgnoreCase));
            }

            if (propertyName is null || !schema.Properties.TryGetValue(propertyName, out var propertySchema))
            {
                continue;
            }

            var rules = memberGroup.Select(v => v.Validator).ToList();

            if (rules.Any(r => r is INotNullValidator || r is INotEmptyValidator))
            {
                schema.Required.Add(propertyName);
            }

            foreach (var rule in rules)
            {
                switch (rule)
                {
                    case ILengthValidator lengthValidator:
                        if (lengthValidator.Max > 0)
                        {
                            propertySchema.MaxLength = lengthValidator.Max;
                        }

                        if (lengthValidator.Min > 0)
                        {
                            propertySchema.MinLength = lengthValidator.Min;
                        }
                        break;

                }
            }
        }
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
