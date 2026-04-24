using Portal.Domain.Base;
using Portal.Domain.Cliente;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.AtualizarCliente;

public class AtualizarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly ILogger<AtualizarClienteHandler> _logger;

    public AtualizarClienteHandler(
        IClienteRepository repository,
        ILogger<AtualizarClienteHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(AtualizarClienteRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando cliente: {Id}", request.Id);

        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<string>("Cliente não encontrado");
        }

        if (await _repository.ExisteCpfAsync(request.Cpf, request.Id, cancellationToken))
        {
            return Result.ValidationResult<string>("Já existe um cliente com este CPF");
        }

        if (await _repository.ExisteEmailAsync(request.Email, request.Id, cancellationToken))
        {
            return Result.ValidationResult<string>("Já existe um cliente com este email");
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
            return Result.NotFoundResult<string>("Cliente não encontrado");
        }

        return Result.OkResult("Cliente atualizado com sucesso");
    }
}
