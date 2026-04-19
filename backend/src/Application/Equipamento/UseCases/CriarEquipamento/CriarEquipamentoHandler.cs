using FluentValidation;
using Portal.Domain.Base;
using Portal.Domain.Equipamento;
using Portal.Domain.Equipamento.Interfaces;

namespace Portal.Application.Equipamento.UseCases.CriarEquipamento;

public class CriarEquipamentoHandler
{
    private readonly IEquipamentoRepository _repository;
    private readonly IValidator<CriarEquipamentoRequest> _validator;

    public CriarEquipamentoHandler(IEquipamentoRepository repository, IValidator<CriarEquipamentoRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<CriarEquipamentoResponse>> Handle(CriarEquipamentoRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result.ValidationResult<CriarEquipamentoResponse>(validation.Errors.Select(x => x.ErrorMessage));
        }

        if (!await _repository.CategoriaExisteAsync(Guid.Parse(request.CategoriaId), cancellationToken))
        {
            return Result.ValidationResult<CriarEquipamentoResponse>("Categoria não encontrada");
        }

        if (await _repository.ExisteNumeroSerieAsync(request.NumeroSerie, cancellationToken: cancellationToken))
        {
            return Result.ValidationResult<CriarEquipamentoResponse>("Já existe um equipamento com este número de série");
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

        var id = await _repository.CriarAsync(entity, cancellationToken);
        return Result.OkResult(new CriarEquipamentoResponse { Id = id });
    }
}
