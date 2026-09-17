using System.Net;

namespace UnitTests.Domain.Clients;

/// <summary>
/// Captures the last outgoing request and returns a canned response, so client code can be
/// tested against the exact URL it builds without making a real network call.
/// </summary>
internal class FakeHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK, HttpContent? content = null) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(statusCode) { Content = content });
    }
}
