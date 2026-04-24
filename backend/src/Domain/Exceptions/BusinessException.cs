namespace Portal.Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public List<string> Errors { get; }

        public BusinessException(List<string> errors)
            : base("Erro de negÃ³cio")
        {
            Errors = errors;
        }

        public BusinessException(string error)
            : base("Erro de negÃ³cio")
        {
            Errors = new List<string> { error };
        }
    }
}
