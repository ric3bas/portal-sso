namespace Portal.Features.Usuario.Domain
{
    public class RegistroValidacao
    {
        public bool ParceiroExiste { get; set; }
        public bool LoginExiste { get; set; }
        public bool PerfilExiste { get; set; }
    }
}
