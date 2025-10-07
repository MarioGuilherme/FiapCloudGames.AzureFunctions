using FiapCloudGames.AzureFunctions.Domain.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;

namespace FiapCloudGames.AzureFunctions.Infrastructure.Services;

public class SendGridEmailService(string apiKey, string senderEmail) : IEmailService
{
    private readonly string _apiKey = apiKey;
    private readonly string _senderEmail = senderEmail;

    public async Task<bool> SendEmailAsync(string recipient, string subject, string htmlContent)
    {
        Log.Information("Enviando email ao destinatário {recipient}", recipient);
        SendGridClient client = new(_apiKey);
        EmailAddress from = new(_senderEmail);
        EmailAddress recipientEmailAddress = new(recipient);
        SendGridMessage sendGridMessage = MailHelper.CreateSingleEmail(from, recipientEmailAddress, subject, string.Empty, htmlContent);
        Response response = await client.SendEmailAsync(sendGridMessage);
        return response.IsSuccessStatusCode;
    }
}
