namespace Portal.Features.Categoria.Domain
{
    public class CategoriaResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    internal static class CategoriaExtensions
    {
        internal static CategoriaResponse ToResponse(this CategoriaEntity categoria)
        {
            return new CategoriaResponse
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Ativo = categoria.Ativo
            };
        }
    }

    public class CategoriaEntity
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public Guid ParceiroId { get; set; }
    }
}