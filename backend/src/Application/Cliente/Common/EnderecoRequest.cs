namespace Portal.Application.Cliente.Common
{
    public class EnderecoRequest
    {
        public string? Id { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
    }
}
