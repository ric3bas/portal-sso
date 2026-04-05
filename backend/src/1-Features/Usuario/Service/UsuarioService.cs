using Portal._1_Features.Usuario.Domain.Responses;
using Portal.Domain.Base;
using Portal.Domain.Exceptions;
using Portal.Features.Perfil.Domain.Interfaces;
using Portal.Features.Usuario.Domain;
using Portal.Features.Usuario.Domain.Interfaces;
using Portal.Features.Usuario.Infra;
using Portal.Infra;
using static Portal.Domain.Base.Result;

namespace Portal.Features.Usuario.Service
{
    public class UsuarioService : BaseService, IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Result<IEnumerable<UsuarioComPerfilResponse>>> ListarPorParceiroAsync(string? parceiroId, CancellationToken cancellationToken = default)
        {
            //check perfil

            var usuario = ObterUsuario();
            var parceiroNull= string.IsNullOrEmpty(parceiroId);

            var perfilMaster = await _usuarioRepository.VerificarUsuarioMasterAsyunc(usuario);
            if(!perfilMaster && !parceiroNull)
                return ValidationResult<IEnumerable<UsuarioComPerfilResponse>>("Usuário não tem permissão");

            var parceiro = parceiroNull ? ObterTenantId() : Guid.Parse(parceiroId ?? string.Empty);
            var result = await _usuarioRepository.ListarPorParceiroAsync(parceiro, cancellationToken);

            if (!result.Any())
               return NotFoundResult<IEnumerable<UsuarioComPerfilResponse>>("Nenhum usuário encontrado");
            return OkResult(result.Select(c=>c.ToResponse()));
        }


        public async Task<Result<string>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());

            var parceiroId = ObterTenantId();
            var validacao  = await _usuarioRepository.ValidarRegistroAsync(request.Login, parceiroId, request.PerfilId, cancellationToken);

            if (!validacao.ParceiroExiste)
                return NotFoundResult<string>($"Parceiro '{parceiroId}' não encontrado");

            if (validacao.LoginExiste)
                return ValidationResult<string>($"Login '{request.Login}' já existe");

            var usuario = new UsuarioCommand
            {
                Nome       = request.Nome,
                Email      = request.Email,
                Login      = request.Login,
                Senha      = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                ParceiroId = parceiroId,
                PerfilId   = request.PerfilId,
                TentativasLogin = 0,
                Bloqueado = false
            };

            _ = await _usuarioRepository.InserirAsync(usuario, cancellationToken);

            return OkResult("Cadastrado com sucesso");
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
            
            await _usuarioRepository.AtualizarAsync(usuario.ToMapper(), cancellationToken);
        }

    }
}
