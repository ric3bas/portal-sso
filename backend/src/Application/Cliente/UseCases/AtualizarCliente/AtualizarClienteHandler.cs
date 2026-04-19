using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Cliente;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.AtualizarCliente;

public class AtualizarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly IValidator<AtualizarClienteRequest> _validator;
    private readonly ILogger<AtualizarClienteHandler> _logger;

    public AtualizarClienteHandler(
        IClienteRepository repository,
        IValidator<AtualizarClienteRequest> validator,
        ILogger<AtualizarClienteHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AtualizarClienteResponse>> Handle(AtualizarClienteRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando cliente: {Id}", request.Id);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.ValidationResult<AtualizarClienteResponse>(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<AtualizarClienteResponse>("Cliente não encontrado");
        }

        if (await _repository.ExisteCpfAsync(request.Cpf, request.Id, cancellationToken))
        {
            return Result.ValidationResult<AtualizarClienteResponse>("Já existe um cliente com este CPF");
        }

        if (await _repository.ExisteEmailAsync(request.Email, request.Id, cancellationToken))
        {
            return Result.ValidationResult<AtualizarClienteResponse>("Já existe um cliente com este email");
        }

        var entity = new ClienteCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Observacao = request.Observacao,
            Bloqueado = request.Bloqueado ?? false,
            Ativo = request.Ativo ?? true,
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

        var linhasAfetadas = await _repository.AtualizarAsync(entity, cancellationToken);
        if (linhasAfetadas == 0)
        {
            return Result.NotFoundResult<AtualizarClienteResponse>("Cliente não encontrado");
        }

        return Result.OkResult(new AtualizarClienteResponse { Mensagem = "Cliente atualizado com sucesso" });
    }
}
