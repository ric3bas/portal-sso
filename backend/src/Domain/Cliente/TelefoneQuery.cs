namespace Portal.Domain.Cliente;

public class TelefoneQuery
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Ddd { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
}
