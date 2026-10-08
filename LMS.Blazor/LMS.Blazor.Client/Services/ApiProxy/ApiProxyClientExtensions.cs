
namespace LMS.Blazor.Client.Services.ApiProxy;

public static class ApiProxyClientExtensions
{
    public static Task<TResponse?> GetAsync<TResponse>(
        this IApiProxyClient client,
        string endpoint,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Get,
            endpoint,
            cancellationToken: cancellationToken);

    public static Task<TResponse?> PostAsync<TResponse>(
        this IApiProxyClient client,
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Post,
            endpoint,
            content,
            cancellationToken);

    public static Task<TResponse?> PutAsync<TResponse>(
        this IApiProxyClient client,
        string endpoint,
        HttpContent content,
        CancellationToken cancellationToken = default) =>
        client.SendAsync<TResponse>(
            HttpMethod.Put,
            endpoint,
            content,
            cancellationToken);
}
