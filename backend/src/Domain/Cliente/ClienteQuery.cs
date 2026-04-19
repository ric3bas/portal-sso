namespace Portal.Domain.Cliente;

public class ClienteQuery
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public bool Bloqueado { get; set; }
    public bool Ativo { get; set; }
    public Guid ParceiroId { get; set; }
    public TelefoneQuery? Telefone { get; set; }
    public EnderecoQuery? Endereco { get; set; }
}
