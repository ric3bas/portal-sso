namespace Portal.Middleware {
    // Middleware não é mais necessário para validação manual do JWT,
    // pois a validação é feita automaticamente pelo JwtBearer.
    // Mantido para compatibilidade, mas apenas chama o próximo middleware.
    public class JwtValidationMiddleware {
        private readonly RequestDelegate _next;
        public JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration) {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context) {
            await _next(context);
        }
    }
}
