using FluentValidation;
using System.Text.RegularExpressions;

namespace Portal.Features.Cliente.Domain.Validations
{
    public class ClienteRequestValidator : AbstractValidator<ClienteRequest>
    {
        public ClienteRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotNull().NotEmpty().WithMessage("Nome do cliente é obrigatório")
                .MinimumLength(2).WithMessage("Nome do cliente deve ter pelo menos 2 caracteres")
                .MaximumLength(200).WithMessage("Nome do cliente não pode ter mais de 200 caracteres");

            RuleFor(x => x.Cpf)
                .NotNull().NotEmpty().WithMessage("CPF é obrigatório")
                .Must(BeValidCpf).WithMessage("CPF inválido");

            RuleFor(x => x.Email)
                .NotNull().NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(200).WithMessage("Email não pode ter mais de 200 caracteres");

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");

            RuleFor(x => x.Telefone)
    .SetValidator(new TelefoneRequestValidator());

            RuleFor(x => x.Endereco)
    .SetValidator(new EnderecoRequestValidator());

        }

        private bool BeValidCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            
            // Remove caracteres não numéricos
            cpf = Regex.Replace(cpf, @"[^\d]", "");
            
            if (cpf.Length != 11) return false;
            
            // Verifica se todos os dígitos são iguais
            if (cpf.All(c => c == cpf[0])) return false;
            
            // Validação do algoritmo do CPF
            var soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            
            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;
            
            if (int.Parse(cpf[9].ToString()) != digito1) return false;
            
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            
            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;
            
            return int.Parse(cpf[10].ToString()) == digito2;
        }
    }

    public class AtualizarClienteRequestValidator : AbstractValidator<AtualizarClienteRequest>
    {
        public AtualizarClienteRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().NotEmpty().WithMessage("Id do cliente é obrigatório");

            RuleFor(x => x.Nome)
                .NotNull().NotEmpty().WithMessage("Nome do cliente é obrigatório")
                .MinimumLength(2).WithMessage("Nome do cliente deve ter pelo menos 2 caracteres")
                .MaximumLength(200).WithMessage("Nome do cliente não pode ter mais de 200 caracteres");

            RuleFor(x => x.Cpf)
                .NotNull().NotEmpty().WithMessage("CPF é obrigatório")
                .Must(BeValidCpf).WithMessage("CPF inválido");

            RuleFor(x => x.Email)
                .NotNull().NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email inválido")
                .MaximumLength(200).WithMessage("Email não pode ter mais de 200 caracteres");

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação não pode ter mais de 500 caracteres");

            RuleFor(x => x.Telefone).SetValidator(new TelefoneRequestValidator());
            RuleFor(x => x.Endereco).SetValidator(new EnderecoRequestValidator());
        }

        private bool BeValidCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            
            cpf = Regex.Replace(cpf, @"[^\d]", "");
            
            if (cpf.Length != 11) return false;
            if (cpf.All(c => c == cpf[0])) return false;
            
            var soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            
            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;
            
            if (int.Parse(cpf[9].ToString()) != digito1) return false;
            
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            
            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;
            
            return int.Parse(cpf[10].ToString()) == digito2;
        }
    }

    public class TelefoneRequestValidator : AbstractValidator<TelefoneRequest>
    {
        public TelefoneRequestValidator()
        {
            RuleFor(x => x.Ddd)
                .NotNull().NotEmpty().WithMessage("DDD é obrigatório")
                .Length(2).WithMessage("DDD deve ter 2 dígitos")
                .Matches(@"^\d{2}$").WithMessage("DDD deve conter apenas números");

            RuleFor(x => x.Numero)
                .NotNull().NotEmpty().WithMessage("Número do telefone é obrigatório")
                .Matches(@"^[0-9]{8,9}$").WithMessage("Número do telefone deve ter 8 ou 9 dígitos");
        }
    }

    public class EnderecoRequestValidator : AbstractValidator<EnderecoRequest>
    {
        public EnderecoRequestValidator()
        {
            RuleFor(x => x.Logradouro)
                .NotNull().NotEmpty().WithMessage("Logradouro é obrigatório")
                .MaximumLength(200).WithMessage("Logradouro não pode ter mais de 200 caracteres");

            RuleFor(x => x.Cidade)
                .NotNull().NotEmpty().WithMessage("Cidade é obrigatória")
                .MaximumLength(100).WithMessage("Cidade não pode ter mais de 100 caracteres");

            RuleFor(x => x.Estado)
                .NotNull().NotEmpty().WithMessage("Estado é obrigatório")
                .Length(2).WithMessage("Estado deve ter 2 caracteres");

            RuleFor(x => x.Numero)
                .NotNull().NotEmpty().WithMessage("Número é obrigatório")
                .MaximumLength(10).WithMessage("Número não pode ter mais de 10 caracteres");

            RuleFor(x => x.Complemento)
                .MaximumLength(100).WithMessage("Complemento não pode ter mais de 100 caracteres");
        }
    }
}