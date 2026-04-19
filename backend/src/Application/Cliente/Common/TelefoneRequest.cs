namespace Portal.Application.Cliente.Common
{
    public class TelefoneRequest
    {
        public string? Id { get; set; }
        public string Ddd { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }
}
