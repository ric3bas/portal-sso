namespace Portal.Domain.Base
{
    public abstract class BaseRequest
    {
        protected FluentValidation.Results.ValidationResult? ValidationResult { get; private set; } 
        public abstract bool IsValid();

        protected bool Validate<T>(T request, FluentValidation.IValidator<T> validator)
        {
            ValidationResult = validator.Validate(request);
            return ValidationResult.IsValid;
        }

        public List<string> ObterErros() {
            if (ValidationResult is null)
                return new List<string> { "Requisição não validada." };

            return ValidationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
        }
    }
}
