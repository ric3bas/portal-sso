namespace Portal.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(List<string> errors)
            : base("Erro de validaÃ§Ã£o")
        {
            Errors = errors;
        }

        public ValidationException(string error)
            : base("Erro de validaÃ§Ã£o")
        {
            Errors = new List<string> { error };
        }
    }
}
