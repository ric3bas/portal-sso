namespace Portal.Features.Usuario.Infra
{
    public class UsuarioQuery
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public Guid ParceiroId { get; set; }
        public int? PerfilId { get; set; }
        public int TentativasLogin { get; set; }
        public bool Bloqueado { get; set; }
        public bool Ativo { get; set; }

        public UsuarioCommand ToMapper()
        {
            return new UsuarioCommand
            {
                Id = this.Id,
                Nome = this.Nome,
                Login = this.Login,
                Email = this.Email,
                Senha = this.Senha,
                ParceiroId = this.ParceiroId,
                PerfilId = this.PerfilId,
                TentativasLogin = this.TentativasLogin,
                Bloqueado = this.Bloqueado
            };
        }
    }
}
