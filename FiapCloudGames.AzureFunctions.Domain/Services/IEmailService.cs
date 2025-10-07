namespace FiapCloudGames.AzureFunctions.Domain.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipient, string subject, string htmlContent);
}