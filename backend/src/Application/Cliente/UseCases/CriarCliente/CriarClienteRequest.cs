using Portal.Application.Cliente.Common;
using FluentValidation;
using Portal.Domain.Base;
using System.Text.RegularExpressions;

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
        var validator = new InlineValidator<CriarClienteRequest>();
        validator.RuleFor(x => x.Nome).NotEmpty().MinimumLength(2).MaximumLength(200);
        validator.RuleFor(x => x.Cpf).NotEmpty().Must(BeValidCpf);
        validator.RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        validator.RuleFor(x => x.Observacao).MaximumLength(500);
        validator.RuleFor(x => x.Telefone.Ddd).NotEmpty().Length(2).Matches("^\\d{2}$");
        validator.RuleFor(x => x.Telefone.Numero).NotEmpty().Matches("^[0-9]{8,9}$");
        validator.RuleFor(x => x.Endereco.Logradouro).NotEmpty().MaximumLength(200);
        validator.RuleFor(x => x.Endereco.Cidade).NotEmpty().MaximumLength(100);
        validator.RuleFor(x => x.Endereco.Estado).NotEmpty().Length(2);
        validator.RuleFor(x => x.Endereco.Numero).NotEmpty().MaximumLength(10);
        validator.RuleFor(x => x.Endereco.Complemento).MaximumLength(100);
        return Validate(this, validator);
    }

    private static bool BeValidCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        cpf = Regex.Replace(cpf, @"[^\d]", "");
        if (cpf.Length != 11) return false;
        if (cpf.All(c => c == cpf[0])) return false;
        var soma = 0;
        for (var i = 0; i < 9; i++) soma += int.Parse(cpf[i].ToString()) * (10 - i);
        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;
        if (int.Parse(cpf[9].ToString()) != digito1) return false;
        soma = 0;
        for (var i = 0; i < 10; i++) soma += int.Parse(cpf[i].ToString()) * (11 - i);
        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;
        return int.Parse(cpf[10].ToString()) == digito2;
    }
}
