using Portal.Application.Cliente.Common;
using Portal.Domain.Base;

namespace Portal.Application.Cliente.UseCases.CriarCliente;

public class CriarClienteRequest : BaseRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Observacao { get; set; }
    public TelefoneRequest Telefone { get; set; } = new();
    public EnderecoRequest Endereco { get; set; } = new();

    public override bool IsValid()
    {
        var validator = new CriarClienteValidator();
        var result = validator.Validate(this);
        return result.IsValid;
    }
}
