using Portal.Dominio;
using Portal.Dominio.Entities;
using Portal.Dominio.Validations;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Infra;

namespace Portal.Features.Usuario.Service
{
    public class UsuarioService : BaseService, IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPerfilRepository  _perfilRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IPerfilRepository perfilRepository,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork) : base(httpContextAccessor)
        {
            _usuarioRepository = usuarioRepository;
            _perfilRepository = perfilRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UsuarioComPerfilResponse>> ListarAsync(CancellationToken cancellationToken = default)
        {
            var parceiroId = ObterTenantId();
            var result = await _usuarioRepository.ListarAsync(parceiroId, cancellationToken);
            if (!result.Any())
                throw new NotFoundException("Nenhum usuário encontrado");
            return result;
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            // 1. Validação dos campos obrigatórios
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            // 2. Validar parceiro, login duplicado e perfil em uma única chamada ao banco
            var parceiroId = ObterTenantId();
            var validacao  = await _usuarioRepository.ValidarRegistroAsync(request.Login, parceiroId, request.PerfilId, cancellationToken);

            if (!validacao.ParceiroExiste)
                throw new NotFoundException($"Parceiro '{parceiroId}' não encontrado");

            if (validacao.LoginExiste)
                throw new ValidationException($"Login '{request.Login}' já existe");

            var usuario = new UsuarioEntity
            {
                Nome       = request.Nome,
                Email      = request.Email,
                Login      = request.Login,
                Senha      = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                ParceiroId = parceiroId,
                PerfilId   = request.PerfilId,
                TentativasLogin = 0,
                UltimoErroLogin = null,
                Bloqueado = false
            };

            // Transação/commit agora é responsabilidade do repositório, se necessário
            _ = await _usuarioRepository.InserirAsync(usuario, cancellationToken);
        }

        public async Task IncrementarTentativaLogin(int usuarioId, CancellationToken cancellationToken)
        {
            await _usuarioRepository.IncrementarTentativaLoginAsync(usuarioId, cancellationToken);
        }

        public async Task ResetarTentativasLogin(int usuarioId, CancellationToken cancellationToken)
        {
            await _usuarioRepository.ResetarTentativasLoginAsync(usuarioId, cancellationToken);
        }

        public async Task BloquearUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
            if (usuario == null)
                throw new NotFoundException("Usuário não encontrado");

            usuario.Bloqueado = true;
            
            await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        }

    }
}
