using Portal.Application.Cliente.UseCases.ObterClientes;

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

    public TResponse ToResponse<TResponse>() where TResponse : ObterClientesResponse, new()
    {
        return new TResponse
        {
            Id = Id,
            Nome = Nome,
            Cpf = Cpf,
            Email = Email,
            Observacao = Observacao,
            Bloqueado = Bloqueado,
            Ativo = Ativo,
            Telefone = Telefone is null ? new TelefoneResponse() : new TelefoneResponse
            {
                Id = Telefone.Id,
                Ddd = Telefone.Ddd,
                Numero = Telefone.Numero
            },
            Endereco = Endereco is null ? new EnderecoResponse() : new EnderecoResponse
            {
                Id = Endereco.Id,
                Logradouro = Endereco.Logradouro,
                Cidade = Endereco.Cidade,
                Estado = Endereco.Estado,
                Numero = Endereco.Numero,
                Complemento = Endereco.Complemento
            }
        };
    }
}
