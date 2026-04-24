using System.Net;
using System.Net.Mail;

namespace Portal.Domain.Base.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpo)
        {
            var smtpSection = _config.GetSection("Smtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"] ?? "25");
            var user = smtpSection["User"];
            var pass = smtpSection["Pass"];
            var from = smtpSection["From"] ?? string.Empty;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };
            var mensagem = new MailMessage(from, destinatario)
            {
                Subject = "AlteraÃ§Ã£o de senha",
                Body = corpo,
                IsBodyHtml = true
            };
            await client.SendMailAsync(mensagem);
        }
    }
}
