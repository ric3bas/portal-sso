using Portal.Domain.Base;
using Portal.Domain.Equipamento;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.CriarEquipamento;

public class CriarEquipamentoHandler
{
    private readonly IEquipamentoRepository _repository;

    public CriarEquipamentoHandler(IEquipamentoRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<string>> Handle(CriarEquipamentoRequest request, CancellationToken cancellationToken)
    {
        if (!request.IsValid()) return Result.ValidationResult<string>(request.ObterErros());

        if (!await _repository.CategoriaExisteAsync(Guid.Parse(request.CategoriaId), cancellationToken))
        {
            return Result.ValidationResult<string>("Categoria não encontrada");
        }

        if (await _repository.ExisteNumeroSerieAsync(request.NumeroSerie, cancellationToken: cancellationToken))
        {
            return Result.ValidationResult<string>("Já existe um equipamento com este número de série");
        }

        var entity = new EquipamentoCommand
        {
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
            Ativo = true
        };

        _ = await _repository.CriarAsync(entity, cancellationToken);
        return Result.OkResult("Equipamento criado com sucesso");
    }
}
