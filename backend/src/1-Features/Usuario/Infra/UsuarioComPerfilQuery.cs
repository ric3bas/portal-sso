using Portal._1_Features.Usuario.Domain.Responses;

namespace Portal.Features.Usuario.Infra
{
    public class UsuarioComPerfilQuery
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Parceiro { get; set; } = string.Empty;
        public string Perfil { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public bool Bloqueado { get; set; }

        public UsuarioComPerfilResponse ToResponse()
        {
            return new UsuarioComPerfilResponse
            {
                Id = this.Id,
                Nome = this.Nome,
                Login = this.Login,
                Email = this.Email,
                Parceiro = this.Parceiro,
                Perfil = this.Perfil,
                Ativo = this.Ativo,
                Bloqueado = this.Bloqueado
            };
        }
    }
}
