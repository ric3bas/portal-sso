namespace Portal.Domain.Base.Email
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario, string assunto, string corpo);
    }
}
