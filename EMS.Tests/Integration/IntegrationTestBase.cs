namespace EMS.Tests.Integration;

using EMS.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

/// <summary>
/// Custom WebApplicationFactory for testing
/// Provides test server and HTTP client for integration tests
/// </summary>
public class EmsTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production DbContext if any
            // Add test-specific services here if needed
        });
    }
}

/// <summary>
/// Base class for integration tests
/// Provides common setup and helper methods
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<EmsTestFactory>
{
    protected readonly HttpClient HttpClient;
    protected readonly EmsTestFactory Factory;

    public IntegrationTestBase(EmsTestFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
    }

    /// <summary>
    /// Get authorization header with token
    /// </summary>
    protected string GetAuthorizationHeader(string token)
    {
        return $"Bearer {token}";
    }

    /// <summary>
    /// Get content from HTTP response
    /// </summary>
    protected async Task<T> GetResponseContent<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
            return default;

        return System.Text.Json.JsonSerializer.Deserialize<T>(content);
    }

    /// <summary>
    /// Send POST request
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data, string authToken = null)
    {
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(data),
            System.Text.Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Add("Authorization", GetAuthorizationHeader(authToken));
        }

        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Send GET request
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string url, string authToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Add("Authorization", GetAuthorizationHeader(authToken));
        }

        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Send PUT request
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<T>(string url, T data, string authToken = null)
    {
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(data),
            System.Text.Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = content
        };

        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Add("Authorization", GetAuthorizationHeader(authToken));
        }

        return await HttpClient.SendAsync(request);
    }

    /// <summary>
    /// Send DELETE request
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url, string authToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);

        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Add("Authorization", GetAuthorizationHeader(authToken));
        }

        return await HttpClient.SendAsync(request);
    }
}
