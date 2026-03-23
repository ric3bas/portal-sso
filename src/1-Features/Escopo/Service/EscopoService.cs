using Portal.Domain.Base;
using Portal.Features.Escopo.Domain;
using Portal.Features.Escopo.Domain.Interfaces;
using Portal.Features.Escopo.Infra;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Escopo.Service
{
    public class EscopoService : IEscopoService
    {
        private readonly IEscopoRepository _repository;
        private readonly ILogger<EscopoService> _logger;

        public EscopoService(IEscopoRepository repository, ILogger<EscopoService> logger)
        {
            _repository = repository;
            _logger     = logger;
        }

        public async Task<Result<IEnumerable<EscopoResponse>>> ListarAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Listando escopos");
            var result = await _repository.ListarAsync(cancellationToken);
            if (!result.Any())
                return ValidationResult<IEnumerable<EscopoResponse>>("Nenhum escopo encontrado");

            return OkResult(result.Select(c => c.ToResponse()));

        }

        public async Task<Result<string>> CriarAsync(string nome, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Criando escopo {nome}");
            var validate = new StringValidator().Validate(nome);
            if (!validate.IsValid) return ValidationResult<string>(validate.Errors.Select(c=>c.ErrorMessage).ToList());

            var nomeExiste = await _repository.ExisteNomeAsync(nome.Trim(), cancellationToken);
            if (nomeExiste)
                return BusinessResult<string>("Nome do escopo já existe");

            var escopo = new EscopoCommand { Nome = nome.Trim() };
            var result = await _repository.InserirAsync(escopo, cancellationToken);
            return OkResult("Criado com sucesso");
        }

        public async Task<Result<EscopoResponse?>> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obtendo escopo {Id}", id);

            if (id <= 0)
                return ValidationResult<EscopoResponse?>("Id do escopo inválido");


            var escopo = await _repository.ObterPorIdAsync(id, cancellationToken);
            if (escopo is null)
                return ValidationResult<EscopoResponse?>("Escopo {id} não encontrado");

            return OkResult<EscopoResponse?>(escopo.ToResponse());

        }
    }
}
