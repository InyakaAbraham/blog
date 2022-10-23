using Blog.Models;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MailKitSmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Blog.Features;

public class EmailService : IEmailService
{
    private readonly AppSettings _appSettings;

    public EmailService(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public async void Send(string to, string subject, string text)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_appSettings.EmailFrom));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Plain) { Text = text };


        using var smtp = new MailKitSmtpClient();
        await smtp.ConnectAsync(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_appSettings.SmtpUser, _appSettings.SmtpPass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}