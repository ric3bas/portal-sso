using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Equipamento;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.AtualizarEquipamento;

public class AtualizarEquipamentoHandler
{
    private readonly IEquipamentoRepository _repository;
    private readonly IValidator<AtualizarEquipamentoRequest> _validator;

    public AtualizarEquipamentoHandler(IEquipamentoRepository repository, IValidator<AtualizarEquipamentoRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<AtualizarEquipamentoResponse>> Handle(AtualizarEquipamentoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.ValidationResult<AtualizarEquipamentoResponse>(validation.Errors.Select(x => x.ErrorMessage));
        }

        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<AtualizarEquipamentoResponse>("Equipamento não encontrado");
        }

        if (!await _repository.CategoriaExisteAsync(Guid.Parse(request.CategoriaId), cancellationToken))
        {
            return Result.ValidationResult<AtualizarEquipamentoResponse>("Categoria não encontrada");
        }

        if (await _repository.ExisteNumeroSerieAsync(request.NumeroSerie, request.Id, cancellationToken))
        {
            return Result.ValidationResult<AtualizarEquipamentoResponse>("Já existe um equipamento com este número de série");
        }

        var entity = new EquipamentoCommand
        {
            Id = request.Id,
            Nome = request.Nome,
            CategoriaId = Guid.Parse(request.CategoriaId),
            QuantidadeEstoque = request.QuantidadeEstoque,
            PrecoDiaria = request.PrecoDiaria,
            Marca = request.Marca,
            Modelo = request.Modelo,
            NumeroSerie = request.NumeroSerie,
            AnoFabricacao = request.AnoFabricacao,
            Descricao = request.Descricao,
            ObservacaoInternas = request.ObservacaoInternas,
            Ativo = request.Ativo
        };

        var rows = await _repository.AtualizarAsync(entity, cancellationToken);
        if (rows == 0)
        {
            return Result.NotFoundResult<AtualizarEquipamentoResponse>("Equipamento não encontrado");
        }

        return Result.OkResult(new AtualizarEquipamentoResponse { Mensagem = "Equipamento atualizado com sucesso" });
    }
}
