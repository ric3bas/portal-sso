namespace Portal.Features.Perfil.Domain
{
    public class PerfilComEscopoResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public List<PerfilEscopoItemResponse> Escopos { get; set; } = new List<PerfilEscopoItemResponse>();
    }

    public class PerfilEscopoItemResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
