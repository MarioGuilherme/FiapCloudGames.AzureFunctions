using FiapCloudGames.AzureFunctions.Domain.Services;
using FiapCloudGames.AzureFunctions.Functions.Middlewares;
using FiapCloudGames.AzureFunctions.Infrastructure.Persistence;
using FiapCloudGames.AzureFunctions.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] (CorrelationId={CorrelationId}) {Message:lj} {NewLine}{Exception}")
    .WriteTo.NewRelicLogs(
        endpointUrl: "https://log-api.newrelic.com/log/v1",
        insertKey: "3F76176BC7D48FB62A252DB7AB0C62708B59B3DCA903928062C304AE327DF776",
        licenseKey: "7a51751afd870f032bf67a4ad3600db4FFFFNRAL",
        applicationName: "FiapCloudGames.AzureFunctions")
    .CreateLogger();

builder
    .ConfigureFunctionsWebApplication()
    .UseMiddleware<CorrelationIdMiddleware>();

builder.Services.AddHttpClient("FiapCloudGamesPaymentsApiClient", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("FiapCloudGamesPaymentsApiUrl")!);
    client.DefaultRequestHeaders.Add("X-Internal-Auth", Environment.GetEnvironmentVariable("GatewayInternalAuth")!);
});
builder.Services.AddHttpClient("FiapCloudGamesGamesApiClient", client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("FiapCloudGamesGamesApiUrl")!);
    client.DefaultRequestHeaders.Add("X-Internal-Auth", Environment.GetEnvironmentVariable("GatewayInternalAuth")!);
});

builder.Logging.AddSerilog();

string fiapCloudGamesUsersConnectionString = builder.Configuration.GetConnectionString("FiapCloudGamesUsersDbContext")!;
string fiapCloudGamesGamesConnectionString = builder.Configuration.GetConnectionString("FiapCloudGamesGamesDbContext")!;
string fiapCloudGamesPaymentsConnectionString = builder.Configuration.GetConnectionString("FiapCloudGamesPaymentsDbContext")!;
builder.Services.AddDbContext<FiapCloudGamesUsersDbContext>(options => options.UseSqlServer(fiapCloudGamesUsersConnectionString));
builder.Services.AddDbContext<FiapCloudGamesGamesDbContext>(options => options.UseSqlServer(fiapCloudGamesGamesConnectionString));
builder.Services.AddDbContext<FiapCloudGamesPaymentsDbContext>(options => options.UseSqlServer(fiapCloudGamesPaymentsConnectionString));
builder.Services.AddSingleton<IEmailService, SendGridEmailService>(_ =>
{
    string apiKey = builder.Configuration.GetValue<string>("SendGrid:ApiKey")!;
    string senderEmail = builder.Configuration.GetValue<string>("SendGrid:SenderEmail")!;
    return new(apiKey, senderEmail);
});

await builder.Build().RunAsync();

Log.CloseAndFlush();
