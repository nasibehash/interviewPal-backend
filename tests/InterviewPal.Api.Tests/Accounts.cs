using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace InterviewPal.Api.Tests;

/// <summary>A registered user in a test: the client, the credentials, the access token and the refresh cookie.</summary>
public record Session(HttpClient Client, string Email, string Password, string AccessToken, string RefreshCookie, JsonElement Body)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public string UserId => Body.GetProperty("user").GetProperty("id").GetString()!;

    public HttpRequestMessage Request(HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);
        return request;
    }

    public Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null) =>
        Client.SendAsync(Request(method, url, body));

    public async Task<JsonElement> GetJson(string url)
    {
        var response = await Send(HttpMethod.Get, url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }
}

public static class Accounts
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // HandleCookies = false: tests carry the refresh cookie by hand, so they can replay old ones
    public static HttpClient NewClient(ApiFactory factory) =>
        factory.CreateClient(new() { HandleCookies = false, AllowAutoRedirect = false });

    public static async Task<Session> Register(ApiFactory factory, string? password = null)
    {
        var client = NewClient(factory);
        var email = $"user-{Guid.NewGuid():N}@example.com";
        password ??= "correct-horse-battery";
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password, displayName = "Test User" }, Json);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        return new Session(client, email, password, body.GetProperty("accessToken").GetString()!,
            CookieValue(RefreshCookieOf(response)!), body);
    }

    public static Task<HttpResponseMessage> Login(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password }, Json);

    public static string? RefreshCookieOf(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith("interviewpal_refresh="))
            : null;

    /// <summary>"name=value" of a Set-Cookie header.</summary>
    public static string CookieValue(string setCookie) => setCookie.Split(';')[0];
}
