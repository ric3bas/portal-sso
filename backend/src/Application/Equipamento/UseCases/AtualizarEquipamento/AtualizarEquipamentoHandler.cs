using Portal.Domain.Base;
using Portal.Domain.Equipamento;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.AtualizarEquipamento;

public class AtualizarEquipamentoHandler
{
    private readonly IEquipamentoRepository _repository;

    public AtualizarEquipamentoHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(AtualizarEquipamentoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (!await _repository.ExisteAsync(request.Id, cancellationToken))
        {
            return Result.NotFoundResult<string>("Equipamento não encontrado");
        }

        if (!await _repository.CategoriaExisteAsync(Guid.Parse(request.CategoriaId), cancellationToken))
        {
            return Result.ValidationResult<string>("Categoria não encontrada");
        }

        if (await _repository.ExisteNumeroSerieAsync(request.NumeroSerie, request.Id, cancellationToken))
        {
            return Result.ValidationResult<string>("Já existe um equipamento com este número de série");
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
            return Result.NotFoundResult<string>("Equipamento não encontrado");
        }

        return Result.OkResult("Equipamento atualizado com sucesso");
    }
}
