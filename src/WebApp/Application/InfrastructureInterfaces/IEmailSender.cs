namespace Application.InfrastructureInterfaces;
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, string? from = null, CancellationToken cancellationToken = default);
}
