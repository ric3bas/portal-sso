namespace Portal.Features.Cliente.Domain
{
    public class ClienteResponse
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

    internal static class ClienteExtensions
    {
        internal static ClienteResponse ToResponse(this ClienteEntity cliente)
        {
            return new ClienteResponse
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Cpf = cliente.Cpf,
                Email = cliente.Email,
                Observacao = cliente.Observacao,
                Bloqueado = cliente.Bloqueado,
                Ativo = cliente.Ativo,
                Telefone = cliente?.Telefone?.ToResponse() ?? new TelefoneResponse(),
                Endereco = cliente?.Endereco?.ToResponse() ?? new EnderecoResponse()
            };
        }

        internal static TelefoneResponse ToResponse(this TelefoneEntity telefone)
        {
            return new TelefoneResponse
            {
                Id = telefone.Id,
                Ddd = telefone.Ddd,
                Numero = telefone.Numero
            };
        }

        internal static EnderecoResponse ToResponse(this EnderecoEntity endereco)
        {
            return new EnderecoResponse
            {
                Id = endereco.Id,
                Logradouro = endereco.Logradouro,
                Cidade = endereco.Cidade,
                Estado = endereco.Estado,
                Numero = endereco.Numero,
                Complemento = endereco.Complemento
            };
        }
    }

    public class ClienteEntity
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public bool Bloqueado { get; set; }
        public bool Ativo { get; set; }
        public Guid ParceiroId { get; set; }
        public TelefoneEntity? Telefone { get; set; }
        public EnderecoEntity? Endereco { get; set; }
    }

    public class TelefoneEntity
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string Ddd { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }

    public class EnderecoEntity
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
    }
}