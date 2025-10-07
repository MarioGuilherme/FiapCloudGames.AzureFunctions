using FiapCloudGames.AzureFunctions.Domain.Entities;
using FiapCloudGames.AzureFunctions.Domain.Services;
using FiapCloudGames.AzureFunctions.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;

namespace FiapCloudGames.AzureFunctions.Functions.Functions;

public class ProcessDueOrdersFunction(
    FiapCloudGamesUsersDbContext fiapCloudGamesUsersDbContext,
    FiapCloudGamesGamesDbContext fiapCloudGamesGamesDbContext,
    IHttpClientFactory httpClientFactory,
    IEmailService emailService
)
{
    private readonly FiapCloudGamesUsersDbContext _fiapCloudGamesUsersDbContext = fiapCloudGamesUsersDbContext;
    private readonly FiapCloudGamesGamesDbContext _fiapCloudGamesGamesDbContext = fiapCloudGamesGamesDbContext;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IEmailService _emailService = emailService;

    [Function(nameof(ProcessDueOrdersFunction))]
    public async Task Run([TimerTrigger("* * */1 * * *")] TimerInfo timer, FunctionContext context)
    {
        Log.Information("Timer trigger disparada às {DateTime}", DateTime.Now);

        Order? order = await _fiapCloudGamesGamesDbContext.Orders
            .AsNoTracking()
            .Include(o => o.Games)
            .FirstOrDefaultAsync(p => p.CanceledAt == null && p.OrderedAt.AddDays(1) < DateTime.Now);

        if (order is null)
        {
            Log.Information("Nenhum pedido vencido pendente de cancelamento");
            return;
        }

        Log.Information("Pedido de Id {OrderId} a ser cancelado", order.OrderId);
        string? correlationId = context.Items["CorrelationId"].ToString();
        using HttpClient httpClientPayments = _httpClientFactory.CreateClient("FiapCloudGamesPaymentsApiClient");
        using HttpClient httpClientGames = _httpClientFactory.CreateClient("FiapCloudGamesGamesApiClient");
        HttpResponseMessage? httpResponseMessage = default;
        httpClientPayments.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        httpClientGames.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        httpResponseMessage = await httpClientPayments.PatchAsync($"/api/payments/{order.PaymentId}/cancel", default);
        if (httpResponseMessage.StatusCode != HttpStatusCode.NoContent)
        {
            Log.Error("Houve um erro durante a solicitação de cancelamento do pagamento de Id {PaymentId}: {StatusCode} - {ReasonPhrase}", order.PaymentId, httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
            return;
        }

        httpResponseMessage = await httpClientGames.PatchAsync($"/api/orders/{order.OrderId}/cancel/", default);
        if (httpResponseMessage.StatusCode != HttpStatusCode.NoContent)
        {
            Log.Error("Houve um erro durante a solicitação de cancelamento do pedido de Id {OrderId}: {StatusCode} - {ReasonPhrase}", order.OrderId, httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
            return;
        }

        User user = await _fiapCloudGamesUsersDbContext.Users.AsNoTracking().FirstAsync(u => u.UserId == order.UserId);

        Log.Information("Enviando email de notificação de compra cancelada ao usuário {email}", user.Email);
        await _emailService.SendEmailAsync(user.Email, "Compra cancelada", "Sua compra foi cancelada devido o seu vencimento de 1 dia");
        Log.Information("Cancelamento do pedido de Id {OrderId} e com Id de pagamento {PaymentId} finalizado.", order.OrderId, order.PaymentId);
    }
}
