namespace Portal.Domain.Base
{
    public abstract class BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected Guid ObterTenantId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("tenantId")?.Value;
            if (!Guid.TryParse(claim, out var tenantId) || tenantId == Guid.Empty)
                throw new UnauthorizedAccessException("TenantId inválido ou ausente no token");
            return tenantId;
        }

        protected string ObterUsuario()
        {
            var usuario = _httpContextAccessor.HttpContext?.User.FindFirst("usuario")?.Value;
            return usuario ?? string.Empty;
        }
    }
}
