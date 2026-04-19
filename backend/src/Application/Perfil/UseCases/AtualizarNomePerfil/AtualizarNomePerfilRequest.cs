namespace Portal.Application.Perfil.UseCases.AtualizarNomePerfil;

public class AtualizarNomePerfilRequest
{
    public int Id { get; set; }
    public string NovoNome { get; set; } = string.Empty;
}
