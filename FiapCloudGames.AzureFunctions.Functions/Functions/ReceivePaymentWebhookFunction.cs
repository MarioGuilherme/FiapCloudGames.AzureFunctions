using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Primitives;
using System.Net;
using Microsoft.EntityFrameworkCore;
using FiapCloudGames.AzureFunctions.Domain.Entities;
using FiapCloudGames.AzureFunctions.Infrastructure.Persistence;
using FiapCloudGames.AzureFunctions.Application.InputModels;
using Serilog;
using FiapCloudGames.AzureFunctions.Domain.Services;

namespace FiapCloudGames.AzureFunctions.Functions.Functions;

public class ReceivePaymentWebhookFunction(
    FiapCloudGamesUsersDbContext fiapCloudGamesUsersDbContext,
    FiapCloudGamesPaymentsDbContext fiapCloudGamesPaymentsDbContext,
    IHttpClientFactory httpClientFactory,
    IEmailService emailService
)
{
    private readonly FiapCloudGamesUsersDbContext _fiapCloudGamesUsersDbContext = fiapCloudGamesUsersDbContext;
    private readonly FiapCloudGamesPaymentsDbContext _fiapCloudGamesPaymentsDbContext = fiapCloudGamesPaymentsDbContext;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IEmailService _emailService = emailService;

    [Function(nameof(ReceivePaymentWebhookFunction))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequest req, FunctionContext context)
    {
        Log.Information("Requisição recebida às {DateTime}", DateTime.Now);

        string keyPakgSeguro = req.Headers.TryGetValue("X-key-webhook", out StringValues value) ? value.ToString() : string.Empty;
        if (keyPakgSeguro != "minhaChaveSecretaDoPagSeguro") // Aqui garanto que só recebo request da PagSeguro, por ser WebHook
        {
            Log.Warning("Tentativa de acesso ao Webhook ReceivePaymentWebhookFunction inválida pela X-key-webhook");
            return new NotFoundResult();
        }

        ReceivedPaymentEvent receivedPaymentEvent = (await req.ReadFromJsonAsync<ReceivedPaymentEvent>())!;
        Log.Information("Desserializado o body com dados: {@ReceivedPaymentEvent}", receivedPaymentEvent);

        Payment payment = await _fiapCloudGamesPaymentsDbContext.Payments.AsNoTracking().FirstAsync(p => p.ExternalId == receivedPaymentEvent.ExternalId);

        string? correlationId = context.Items["CorrelationId"].ToString();
        using HttpClient httpClientPayments = _httpClientFactory.CreateClient("FiapCloudGamesPaymentsApiClient");
        using HttpClient httpClientGames = _httpClientFactory.CreateClient("FiapCloudGamesGamesApiClient");
        HttpResponseMessage? httpResponseMessage = default;
        httpClientPayments.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        httpClientGames.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);
        User user = await _fiapCloudGamesUsersDbContext.Users.AsNoTracking().FirstAsync(u => u.UserId == payment.UserId);

        Log.Information("Enviado requisição ao serviço de Pagamentos para marcá-lo como pago. Pagamento Id {PaymentId}", payment.PaymentId);
        httpResponseMessage = await httpClientPayments.PatchAsync($"/api/payments/{payment.PaymentId}/mark-as-paid", default);
        if (httpResponseMessage.StatusCode != HttpStatusCode.NoContent)
        {
            Log.Error("Houve um erro durante a solicitação de atualizar o pagamento para pago para o pagamento de Id {PaymentId}: {StatusCode} - {ReasonPhrase}", payment.PaymentId, httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
            return new StatusCodeResult(500);
        }

        Log.Information("Enviado requisição ao serviço de Jogos para desbloquear os jogos na biblioteca do usuário. Pedido Id {OrderId}", payment.OrderId);
        httpResponseMessage = await httpClientGames.PatchAsync($"/api/orders/{payment.OrderId}/unlock-games", default);
        if (httpResponseMessage.StatusCode != HttpStatusCode.NoContent)
        {
            Log.Error("Houve um erro durante a solicitação de debloqueio dos jogos do pedido de Id {OrderId}: {StatusCode} - {ReasonPhrase}", payment.OrderId, httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase);
            return new StatusCodeResult(500);
        }

        await _emailService.SendEmailAsync(user.Email, "Compra Paga", "Recebemos seu pagamento. Seus jogos estão disponível em sua biblioteca");
        Log.Information("Webhook de recepção de pagamento finalizado");
        return new NoContentResult();
    }
}
