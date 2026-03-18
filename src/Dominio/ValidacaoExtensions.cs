using FluentValidation;

namespace Portal.Dominio
{
    public static class ValidacaoExtensions
    {
        public static IRuleBuilderOptions<T, string?> AplicaRegraMaximoCaracteres<T>(this IRuleBuilder<T, string?> rule, int maxCaracteres)
        {
            var newRule = rule.MaximumLength(maxCaracteres);

            return newRule.WithMessage("Limite máximo de caracteres excedido do Campo {PropertyName}. Qtd enviada: {TotalLength}, Máximo: {MaxLength}");
        }
        
        public static IRuleBuilderOptions<T, string?> AplicaRegraMinimoCaracteres<T>(this IRuleBuilder<T, string?> rule, int minCaracteres)
        {
            var newRule = rule.MinimumLength(minCaracteres);

            return newRule.WithMessage("Limite minímo de caracteres não atingido do Campo {PropertyName}. Qtd enviada: {TotalLength}, Minímo: {MinLength}");
        }
        public static IRuleBuilderOptions<T, TProperty> AplicaRegraCampoObrigatorio<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, bool>? condicional = null)
        {
            var newRule = rule.NotEmpty();
    
            if (condicional != null)
            {
                newRule.When(condicional);
            }
    
            return newRule.WithMessage("Campo {PropertyName} obrigatório").WithErrorCode("0001");
        }
    }
}