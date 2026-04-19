namespace Portal.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(List<string> errors)
            : base("Erro de validação")
        {
            Errors = errors;
        }

        public ValidationException(string error)
            : base("Erro de validação")
        {
            Errors = new List<string> { error };
        }
    }
}
