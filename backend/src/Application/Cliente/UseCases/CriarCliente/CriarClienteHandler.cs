using FluentValidation;
using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Cliente;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.CriarCliente;

public class CriarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly IValidator<CriarClienteRequest> _validator;
    private readonly ILogger<CriarClienteHandler> _logger;

    public CriarClienteHandler(
        IClienteRepository repository,
        IValidator<CriarClienteRequest> validator,
        ILogger<CriarClienteHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CriarClienteResponse>> Handle(CriarClienteRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando cliente: {Nome}", request.Nome);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.ValidationResult<CriarClienteResponse>(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        if (await _repository.ExisteCpfAsync(request.Cpf, cancellationToken: cancellationToken))
        {
            return Result.ValidationResult<CriarClienteResponse>("Já existe um cliente com este CPF");
        }

        if (await _repository.ExisteEmailAsync(request.Email, cancellationToken: cancellationToken))
        {
            return Result.ValidationResult<CriarClienteResponse>("Já existe um cliente com este email");
        }

        var entity = new ClienteCommand
        {
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Observacao = request.Observacao,
            Bloqueado = false,
            Ativo = true,
            Telefone = new TelefoneCommand
            {
                Ddd = request.Telefone.Ddd,
                Numero = request.Telefone.Numero
            },
            Endereco = new EnderecoCommand
            {
                Logradouro = request.Endereco.Logradouro,
                Cidade = request.Endereco.Cidade,
                Estado = request.Endereco.Estado,
                Numero = request.Endereco.Numero,
                Complemento = request.Endereco.Complemento
            }
        };

        var id = await _repository.CriarAsync(entity, cancellationToken);
        return Result.OkResult(new CriarClienteResponse
        {
            Id = id,
            Mensagem = "Cliente criado com sucesso"
        });
    }
}
