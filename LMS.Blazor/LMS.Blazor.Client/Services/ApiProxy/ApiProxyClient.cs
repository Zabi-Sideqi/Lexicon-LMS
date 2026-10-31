using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;

namespace LMS.Blazor.Client.Services.ApiProxy;

    public sealed class ApiProxyClient(
    HttpClient httpClient,
    NavigationManager navigationManager) : IApiProxyClient
{
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null,
        CancellationToken cancellationToken = default)
    {
        var proxyUri = GetProxyUri(endpoint);
        using var request = new HttpRequestMessage(method, proxyUri)
        {
            Content = content
        };

        if (RequiresAntiforgery(method))
        {
            var requestToken = await GetAntiforgeryTokenAsync(cancellationToken);
            request.Headers.Add(AntiforgeryProtocol.HeaderName, requestToken);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);

        HandleAuthorizationFailure(response);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var message = string.IsNullOrWhiteSpace(errorBody)
                ? $"Request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase})."
                : errorBody;

            throw new HttpRequestException(message, null, response.StatusCode);
        }

        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    private async Task<string> GetAntiforgeryTokenAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            AntiforgeryProtocol.TokenEndpoint,
            cancellationToken);

        HandleAuthorizationFailure(response);
        response.EnsureSuccessStatusCode();

        var token = await response.Content
            .ReadFromJsonAsync<AntiforgeryTokenDto>(cancellationToken);

        return !string.IsNullOrWhiteSpace(token?.RequestToken)
            ? token.RequestToken
            : throw new InvalidOperationException(
                "The BFF returned an invalid antiforgery token response.");
    }

    private void HandleAuthorizationFailure(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var returnUrl = Uri.EscapeDataString(_navigationManager.Uri);

            _navigationManager.NavigateTo(
                $"Account/Login?returnUrl={returnUrl}",
                forceLoad: true);

            throw new UnauthorizedAccessException(
                "The Blazor session is no longer valid.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            _navigationManager.NavigateTo(
                "Account/AccessDenied",
                forceLoad: true);
        }
    }

    private static bool RequiresAntiforgery(HttpMethod method) =>
        method != HttpMethod.Get &&
        method != HttpMethod.Head &&
        method != HttpMethod.Options &&
        method != HttpMethod.Trace;

    private static string GetProxyUri(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint) ||
            endpoint.StartsWith('/') ||
            endpoint.StartsWith('\\') ||
            endpoint.Contains('\\') ||
            endpoint.Contains('#') ||
            Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            throw new ArgumentException(
                "A non-empty relative API path is required.",
                nameof(endpoint));
        }

        var path = endpoint.Split('?', 2)[0];
        if (Uri.UnescapeDataString(path)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException(
                "Relative API paths cannot contain traversal segments.",
                nameof(endpoint));
        }

        return $"api/proxy/{endpoint}";
    }
}
