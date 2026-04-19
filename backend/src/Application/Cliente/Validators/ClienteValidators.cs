using FluentValidation;
using Portal.Application.Cliente.Common;
using Portal.Application.Cliente.UseCases.AtualizarCliente;
using Portal.Application.Cliente.UseCases.CriarCliente;
using System.Text.RegularExpressions;

namespace Portal.Application.Cliente.Validators;

public class CriarClienteValidator : AbstractValidator<CriarClienteRequest>
{
    public CriarClienteValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório")
            .MinimumLength(2).WithMessage("Nome do cliente deve ter pelo menos 2 caracteres")
            .MaximumLength(200).WithMessage("Nome do cliente não pode ter mais de 200 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Must(BeValidCpf).WithMessage("CPF inválido");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(200).WithMessage("Email não pode ter mais de 200 caracteres");

        RuleFor(x => x.Observacao)
            .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");

        RuleFor(x => x.Telefone).SetValidator(new TelefoneRequestValidator());
        RuleFor(x => x.Endereco).SetValidator(new EnderecoRequestValidator());
    }

    private static bool BeValidCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        cpf = Regex.Replace(cpf, @"[^\d]", "");

        if (cpf.Length != 11) return false;
        if (cpf.All(c => c == cpf[0])) return false;

        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += int.Parse(cpf[i].ToString()) * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != digito1) return false;

        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += int.Parse(cpf[i].ToString()) * (11 - i);

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == digito2;
    }
}

public class AtualizarClienteValidator : AbstractValidator<AtualizarClienteRequest>
{
    public AtualizarClienteValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id do cliente é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório")
            .MinimumLength(2).WithMessage("Nome do cliente deve ter pelo menos 2 caracteres")
            .MaximumLength(200).WithMessage("Nome do cliente não pode ter mais de 200 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Must(BeValidCpf).WithMessage("CPF inválido");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(200).WithMessage("Email não pode ter mais de 200 caracteres");

        RuleFor(x => x.Observacao)
            .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");

        RuleFor(x => x.Telefone).SetValidator(new TelefoneRequestValidator());
        RuleFor(x => x.Endereco).SetValidator(new EnderecoRequestValidator());
    }

    private static bool BeValidCpf(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        cpf = Regex.Replace(cpf, @"[^\d]", "");

        if (cpf.Length != 11) return false;
        if (cpf.All(c => c == cpf[0])) return false;

        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += int.Parse(cpf[i].ToString()) * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != digito1) return false;

        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += int.Parse(cpf[i].ToString()) * (11 - i);

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == digito2;
    }
}
