using Portal.Application.Cliente.Common;
using Portal.Domain.Base;
using Portal.Domain.Cliente;
using Portal.Domain.Cliente.Interfaces;

namespace Portal.Application.Cliente.UseCases.CriarCliente;

public class CriarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly ILogger<CriarClienteHandler> _logger;

    public CriarClienteHandler(
        IClienteRepository repository,
        ILogger<CriarClienteHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(CriarClienteRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando cliente: {Nome}", request.Nome);

        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (await _repository.ExisteCpfAsync(request.Cpf, cancellationToken: cancellationToken))
        {
            return Result.ValidationResult<string>("Já existe um cliente com este CPF");
        }

        if (await _repository.ExisteEmailAsync(request.Email, cancellationToken: cancellationToken))
        {
            return Result.ValidationResult<string>("Já existe um cliente com este email");
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

        _ = await _repository.CriarAsync(entity, cancellationToken);
        return Result.OkResult("Cliente criado com sucesso");
    }
}
