using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Domain.Clients;
using Domain.Constants;

namespace Domain.DependencyInjection;

public static class HttpClientFactorySetup
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(HttpClientSettings.DEFAULT_TIMEOUT_SECONDS);

    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        // TODO Use const value for User-Agent with app version
        services.AddHttpClient<IArchidektClient, ArchidektClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.ARCHIDEKT_BASE_URL);
            client.Timeout = DefaultTimeout;
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IMoxfieldClient, MoxfieldClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.MOXFIELD_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", "MagicProxyPrinter");
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IEdhrecClient, EdhrecClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.EDHREC_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", "MagicProxyPrinter");
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGoldfishClient, GoldfishClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.GOLDFISH_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", "MagicProxyPrinter");
            client.DefaultRequestHeaders.Add("Accept", "text/html");
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ICubeCobraClient, CubeCobraClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.CUBECOBRA_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", "MagicProxyPrinter");
            client.DefaultRequestHeaders.Add("Accept", "text/html");
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IScryfallClient, ScryfallClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.SCRYFALL_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", "MagicProxyPrinter");
            client.DefaultRequestHeaders.Add("Accept", "text/json");
        })
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()  // Handles 5xx, 408 and other transient errors
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Retry on 429 (Too Many Requests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));  // Exponential backoff
    }
}