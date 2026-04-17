using Portal.Domain.Base;
using Portal.Features.Cliente.Domain.Validations;
using System.Text.Json.Serialization;

namespace Portal.Features.Cliente.Domain
{
    public class ClienteRequest : BaseRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public TelefoneRequest Telefone { get; set; } = new();
        public EnderecoRequest Endereco { get; set; } = new();

        [JsonIgnore]
        public Guid ParceiroId { get; set; }

        public override bool IsValid()
        {
            var validator = new ClienteRequestValidator();
            var result = validator.Validate(this);
            return result.IsValid;
        }
    }

    public class AtualizarClienteRequest : BaseRequest
    {
        public string? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Observacao { get; set; }
        public bool? Bloqueado { get; set; }
        public bool? Ativo { get; set; }
        public TelefoneRequest Telefone { get; set; } = new();
        public EnderecoRequest Endereco { get; set; } = new();

        public override bool IsValid()
        {
            var validator = new AtualizarClienteRequestValidator();
            var result = validator.Validate(this);
            return result.IsValid;
        }
    }

    public class TelefoneRequest
    {
        public string? Id { get; set; }
        public string Ddd { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }

    public class EnderecoRequest
    {
        public string? Id { get; set; }
        public string Logradouro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
    }

    public class FiltroClienteRequest
    {
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
    }
}