using System.Security.Claims;

namespace Portal.Domain.Base
{
    public abstract class BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //protected Guid ObterTenantId()
        //{
        //    var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value;
        //    if (!Guid.TryParse(claim, out var tenantId) || tenantId == Guid.Empty)
        //        throw new UnauthorizedAccessException("TenantId inválido ou ausente no token");
        //    return tenantId;
        //}

        //protected bool ObterMaster()
        //{
        //    var isMaster = _httpContextAccessor.HttpContext?.User.FindFirst("isMaster")?.Value;
        //    return Convert.ToBoolean(isMaster);
        //}

        public UsuarioDto ObterUsuario()
        {
            var isMaster = Convert.ToBoolean(_httpContextAccessor.HttpContext?.User.FindFirst("isMaster")?.Value);
            Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value, out var tenantId);
            var parceiroId = tenantId;
            var usuario = _httpContextAccessor.HttpContext?.User.FindFirst("usuario")?.Value;

            return new UsuarioDto()
            {
                IsMaster = isMaster,
                ParceiroId = parceiroId,
                Usuario = usuario ?? string.Empty
            };
        }
    }

    public class UsuarioDto
    {
        public string? Usuario{ get; set; }
        public Guid ParceiroId { get; set; }
        public bool IsMaster { get; set; }
    }
}
