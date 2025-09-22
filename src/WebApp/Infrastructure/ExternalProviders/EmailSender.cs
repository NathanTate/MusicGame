using Domain.Interfaces;
using Domain.Primitives;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Infrastructure.ExternalProviders;
internal class EmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpOptions;
    private readonly ILogger _logger;
    public EmailSender(IOptions<SmtpSettings> smtpOptions, ILogger<EmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
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

            _logger.LogInformation($"SmtpSetting {JsonSerializer.Serialize(_smtpOptions)}");

            emailClient.EnableSsl = true;

            emailClient.Timeout = 5000;

            await emailClient.SendMailAsync(message, cancellationToken);
        }
    }
}
