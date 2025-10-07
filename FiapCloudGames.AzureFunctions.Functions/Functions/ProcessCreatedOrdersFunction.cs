using FiapCloudGames.AzureFunctions.Domain.Entities;
using FiapCloudGames.AzureFunctions.Domain.Services;
using FiapCloudGames.AzureFunctions.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;
using System.Net.Http.Json;

namespace FiapCloudGames.AzureFunctions.Functions.Functions;

public class ProcessCreatedOrdersFunction(
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

    [Function(nameof(ProcessCreatedOrdersFunction))]
    public async Task Run([TimerTrigger("* * */1 * * *")] TimerInfo timerInfo, FunctionContext context)
    {
        Log.Information("Timer trigger disparada às {DateTime}", DateTime.Now);
        Order? order = await _fiapCloudGamesGamesDbContext.Orders
            .AsNoTracking()
            .Include(o => o.Games)
            .FirstOrDefaultAsync(p => p.PaymentId == null && p.OrderedAt.AddDays(1) >= DateTime.Now);

        if (order is null)
        {
            Log.Information("Nenhum pedido pendente de processamento");
            return;
        }

        Log.Information("Pedido de Id {OrderId} a ser processado", order.OrderId);

        string? correlationId = context.Items["CorrelationId"].ToString();
        using HttpClient httpClientPayments = _httpClientFactory.CreateClient("FiapCloudGamesPaymentsApiClient");
        using HttpClient httpClientGames = _httpClientFactory.CreateClient("FiapCloudGamesGamesApiClient");
        HttpResponseMessage? httpResponseMessage = default;
        httpClientPayments.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        httpClientGames.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        User user = await _fiapCloudGamesUsersDbContext.Users.AsNoTracking().FirstAsync(u => u.UserId == order.UserId);

        httpResponseMessage = await httpClientPayments.PostAsJsonAsync("/api/payments", new
        {
            order.OrderId,
            order.UserId,
            Total = order.Games.Sum(g => g.Price)
        });
        if (httpResponseMessage.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            Log.Warning("O Pedido de Id {OrderId} foi identificado como fraudulento. Iniciando pedido de cancelamento", order.OrderId);
            await _emailService.SendEmailAsync(user.Email, "Compra fraudulenta", "Sua compra foi identificada como fraudulenta");
            await httpClientGames.PatchAsync($"/api/orders/{order.OrderId}/cancel/", default);
            return;
        }

        if (httpResponseMessage.StatusCode != HttpStatusCode.Created)
        {
            Log.Error("Houve um erro durante a solicitação de criação de pagamento para o pedido de Id {OrderId}: {StatusCode} - {ReasonPhrase}", order.OrderId, httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
            return;
        }

        int paymentId = int.Parse(httpResponseMessage.Headers.Location!.ToString().Split('/').Last());
        Log.Information("Pagamento pendente criado para o pedido de Id {OrderId}. Id do pagamento {PaymentId}", order.OrderId, paymentId);

        Log.Information("Iniciando ação de atualização do Id do pagamento no banco no serviço de Games");
        httpResponseMessage = await httpClientGames.PatchAsync($"/api/orders/{order.OrderId}/update-paymentId/{paymentId}", default);
        if (httpResponseMessage.StatusCode != HttpStatusCode.NoContent)
        {
            Log.Error("Houve um erro durante a solicitação de atualização do id do pagamento para o pedido: {StatusCode} - {ReasonPhrase}", httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
            return;
        }

        await _emailService.SendEmailAsync(user.Email, "Compra criada", "Sua compra foi criada e está pendente de pagamento");
        Log.Information("Processamento do pedido de Id {OrderId} finalizado.", order.OrderId);
    }
}
