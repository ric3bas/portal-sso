namespace Portal.Domain.Exceptions
{
    public class BusinessException : Exception
    {
        public List<string> Errors { get; }

        public BusinessException(List<string> errors)
            : base("Erro de negócio")
        {
            Errors = errors;
        }

        public BusinessException(string error)
            : base("Erro de negócio")
        {
            Errors = new List<string> { error };
        }
    }
}
