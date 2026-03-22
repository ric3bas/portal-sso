namespace Portal.Features.Perfil.Domain
{
    public sealed class PerfilEscopoRowResponse
    {
        public int PerfilId { get; set; }
        public string PerfilNome { get; set; } = string.Empty;
        public int? EscopoId { get; set; }
        public string? EscopoNome { get; set; }
    }
}
