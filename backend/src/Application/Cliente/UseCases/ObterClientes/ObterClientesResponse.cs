
namespace Portal.Application.Cliente.UseCases.ObterClientes;

public class ObterClientesResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public bool Bloqueado { get; set; }
    public bool Ativo { get; set; }
    public TelefoneResponse Telefone { get; set; } = new();
    public EnderecoResponse Endereco { get; set; } = new();
}

public class TelefoneResponse
{
    public Guid Id { get; set; }
    public string Ddd { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
}

public class EnderecoResponse
{
    public Guid Id { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
}
