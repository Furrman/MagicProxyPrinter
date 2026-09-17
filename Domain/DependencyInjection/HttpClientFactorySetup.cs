using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Domain.Clients;
using Domain.Constants;

namespace Domain.DependencyInjection;

public static class HttpClientFactorySetup
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(HttpClientSettings.DEFAULT_TIMEOUT_SECONDS);
    private static readonly string UserAgent = BuildUserAgent();

    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IArchidektClient, ArchidektClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.ARCHIDEKT_BASE_URL);
            client.Timeout = DefaultTimeout;
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IMoxfieldClient, MoxfieldClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.MOXFIELD_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IEdhrecClient, EdhrecClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.EDHREC_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGoldfishClient, GoldfishClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.GOLDFISH_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.DefaultRequestHeaders.Add("Accept", HttpClientSettings.ACCEPT_HTML);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ICubeCobraClient, CubeCobraClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.CUBECOBRA_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.DefaultRequestHeaders.Add("Accept", HttpClientSettings.ACCEPT_HTML);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IScryfallClient, ScryfallClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.SCRYFALL_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            client.DefaultRequestHeaders.Add("Accept", HttpClientSettings.ACCEPT_JSON);
        })
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static string BuildUserAgent()
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version is null
            ? HttpClientSettings.APP_NAME
            : $"{HttpClientSettings.APP_NAME}/{version.ToString(3)}";
    }

    private static AsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()  // Handles 5xx, 408 and other transient errors
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Retry on 429 (Too Many Requests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));  // Exponential backoff
    }
}