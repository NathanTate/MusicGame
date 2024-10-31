using Application.InfrastructureInterfaces;
using Domain.Primitives;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.ExternalProviders;
internal class EmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpOptions;
    public EmailSender(IOptions<SmtpSettings> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }
    public async Task SendAsync(string to, string subject, string body, string? from = null, CancellationToken cancellationToken = default)
    {
        var message = new MailMessage(
            from ?? _smtpOptions.From,
            to, 
            subject, 
            body);

        using (var emailClient = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port))
        {
            emailClient.Credentials = new NetworkCredential(
                _smtpOptions.User,
                _smtpOptions.Password);

            await emailClient.SendMailAsync(message, cancellationToken);
        }
    }
}
