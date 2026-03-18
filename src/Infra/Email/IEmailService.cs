using System.Threading.Tasks;

namespace Portal.Infra.Email
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario, string assunto, string corpo);
    }
}
