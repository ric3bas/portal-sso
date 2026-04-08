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
        private readonly IPerfilRepository _perfiRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IHttpContextAccessor httpContextAccessor,
            IPerfilRepository perfiRepository,
            IUnitOfWork unitOfWork) : base(httpContextAccessor)
        {
            _usuarioRepository = usuarioRepository;
            _perfiRepository = perfiRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<UsuarioComPerfilResponse>>> ListarPorParceiroAsync(string? parceiroId, CancellationToken cancellationToken = default)
        {
            //check perfil

            var usuario = ObterUsuario();
            Guid.TryParse(parceiroId, out var parceiroParsed);
            Guid IdParceiroQuery = usuario.IsMaster && string.IsNullOrEmpty(parceiroId) 
                ? Guid.Empty 
                : !usuario.IsMaster && string.IsNullOrEmpty(parceiroId) ? usuario.ParceiroId : parceiroParsed;


            //var perfilMaster = await _usuarioRepository.VerificarUsuarioMasterAsyunc(usuario);
            //if(!perfilMaster && !parceiroNull)
            //    return ValidationResult<IEnumerable<UsuarioComPerfilResponse>>("Usuário não tem permissão");

            //var parceiro = parceiroNull ? ObterTenantId() : Guid.Parse(parceiroId ?? string.Empty);
            var result = await _usuarioRepository.ListarPorParceiroAsync(IdParceiroQuery, cancellationToken);

            if (!result.Any())
               return NotFoundResult<IEnumerable<UsuarioComPerfilResponse>>("Nenhum usuário encontrado");
            return OkResult(result.Select(c=>c.ToResponse()));
        }


        public async Task<Result<string>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
                throw new ValidationException(request.ObterErros());
            Guid.TryParse(request.ParceiroId, out var parceiroParsed);

            var validacao  = await _usuarioRepository.ValidarRegistroAsync(request.Login, parceiroParsed, request.PerfilId, cancellationToken);

            if (!validacao.ParceiroExiste)
                return NotFoundResult<string>($"Parceiro '{parceiroParsed}' não encontrado");

            if (validacao.LoginExiste)
                return ValidationResult<string>($"Login '{request.Login}' já existe");

            var usuario = new UsuarioCommand
            {
                Nome       = request.Nome,
                Email      = request.Email,
                Login      = request.Login,
                Senha      = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                ParceiroId = parceiroParsed,
                PerfilId   = request.PerfilId,
                TentativasLogin = 0,
                Bloqueado = false
            };

            _unitOfWork.Begin();
            try
            {
                _ = await _usuarioRepository.InserirAsync(usuario, cancellationToken);
                var escoposIds = await _perfiRepository.ObterEscoposPorPerfilAsync(request.PerfilId, cancellationToken);
                if(escoposIds is not null)
                    await _perfiRepository.VincularEscoposAsync(request.PerfilId, escoposIds, cancellationToken);


                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                return ValidationResult<string>($"Erro ao criar usuário");
            }

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

        public async Task<Result<string>> AtualizarAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id, cancellationToken);
            if (usuario == null)
                return NotFoundResult<string>("Usuário não encontrado");

            await _usuarioRepository.AtualizarAsync(id, request, cancellationToken);
            return OkResult("Alterado com sucesso");
        }
    }
}
