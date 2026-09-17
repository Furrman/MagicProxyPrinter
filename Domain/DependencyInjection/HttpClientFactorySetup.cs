using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Domain.Clients;
using Domain.Constants;

namespace Domain.DependencyInjection;

public static class HttpClientFactorySetup
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(HttpClientSettings.DEFAULT_TIMEOUT_SECONDS);

    /// <param name="appVersion">
    /// The hosting application's version (e.g. "1.2.3"), sent as part of the User-Agent header.
    /// Pass <c>null</c> (or leave it out) to send just the app name, without a version.
    /// </param>
    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, string? appVersion = null)
    {
        var userAgent = BuildUserAgent(appVersion);

        services.AddHttpClient<IArchidektClient, ArchidektClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.ARCHIDEKT_BASE_URL);
            client.Timeout = DefaultTimeout;
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IMoxfieldClient, MoxfieldClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.MOXFIELD_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IEdhrecClient, EdhrecClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.EDHREC_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IGoldfishClient, GoldfishClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.GOLDFISH_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("Accept", HttpClientSettings.ACCEPT_HTML);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ICubeCobraClient, CubeCobraClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.CUBECOBRA_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("Accept", HttpClientSettings.ACCEPT_HTML);
        }).AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IScryfallClient, ScryfallClient>(client =>
        {
            client.BaseAddress = new Uri(HttpClientSettings.SCRYFALL_BASE_URL);
            client.Timeout = DefaultTimeout;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("Accept", HttpClientSettings.ACCEPT_JSON);
        })
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static string BuildUserAgent(string? appVersion)
    {
        return string.IsNullOrEmpty(appVersion)
            ? HttpClientSettings.APP_NAME
            : $"{HttpClientSettings.APP_NAME}/{appVersion}";
    }

    private static AsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()  // Handles 5xx, 408 and other transient errors
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Retry on 429 (Too Many Requests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));  // Exponential backoff
    }
}