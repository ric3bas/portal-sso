using Microsoft.Extensions.Configuration;
using NSubstitute;
using Portal.Domain.Base.Email;

namespace sso.global;

public class SmtpEmailServiceTests
{
    [Fact]
    public async Task EnviarEmailAsync_ComPortaInvalida_DeveLancarFormatException()
    {
        var config = CriarConfiguracaoSmtp(host: "smtp.teste.com", port: "porta-invalida", user: "user", pass: "pass", from: "noreply@teste.com");
        var service = new SmtpEmailService(config);

        await Assert.ThrowsAsync<FormatException>(() => service.EnviarEmailAsync("destinatario@teste.com", "Assunto", "Corpo"));
    }

    [Fact]
    public async Task EnviarEmailAsync_ComFromAusente_DeveLancarArgumentException()
    {
        var config = CriarConfiguracaoSmtp(host: "smtp.teste.com", port: "25", user: "user", pass: "pass", from: null);
        var service = new SmtpEmailService(config);

        await Assert.ThrowsAsync<ArgumentException>(() => service.EnviarEmailAsync("destinatario@teste.com", "Assunto", "Corpo"));
    }

    private static IConfiguration CriarConfiguracaoSmtp(string? host, string? port, string? user, string? pass, string? from)
    {
        var config = Substitute.For<IConfiguration>();
        var smtpSection = Substitute.For<IConfigurationSection>();

        config.GetSection("Smtp").Returns(smtpSection);
        smtpSection["Host"].Returns(host);
        smtpSection["Port"].Returns(port);
        smtpSection["User"].Returns(user);
        smtpSection["Pass"].Returns(pass);
        smtpSection["From"].Returns(from);

        return config;
    }
}
