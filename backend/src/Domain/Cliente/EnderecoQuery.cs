namespace Portal.Domain.Cliente;

public class EnderecoQuery
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
}
