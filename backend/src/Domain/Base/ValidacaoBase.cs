using FluentValidation;

namespace Portal.Domain.Base
{
    public static class ValidacaoBase
    {
        public static IRuleBuilderOptions<T, string?> AplicaRegraMaximoCaracteres<T>(this IRuleBuilder<T, string?> rule, int maxCaracteres)
        {
            var newRule = rule.MaximumLength(maxCaracteres);

            return newRule.WithMessage("Limite mÃ¡ximo de caracteres excedido do Campo {PropertyName}. Qtd enviada: {TotalLength}, MÃ¡ximo: {MaxLength}");
        }
        
        public static IRuleBuilderOptions<T, string?> AplicaRegraMinimoCaracteres<T>(this IRuleBuilder<T, string?> rule, int minCaracteres)
        {
            var newRule = rule.MinimumLength(minCaracteres);

            return newRule.WithMessage("Limite minÃ­mo de caracteres nÃ£o atingido do Campo {PropertyName}. Qtd enviada: {TotalLength}, MinÃ­mo: {MinLength}");
        }
        public static IRuleBuilderOptions<T, TProperty> AplicaRegraCampoObrigatorio<T, TProperty>(this IRuleBuilder<T, TProperty> rule, Func<T, bool>? condicional = null)
        {
            var newRule = rule.NotEmpty();
    
            if (condicional != null)
            {
                newRule.When(condicional);
            }
    
            return newRule.WithMessage("Campo {PropertyName} obrigatÃ³rio").WithErrorCode("0001");
        }
    }
}
