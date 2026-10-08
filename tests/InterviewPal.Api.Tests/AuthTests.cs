using System.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Api.Tests;

public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static HttpRequestMessage RefreshRequest(string cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", cookie);
        return request;
    }

    [Fact]
    public async Task Register_returns_a_token_and_sets_an_httponly_strict_refresh_cookie()
    {
        var client = Accounts.NewClient(factory);
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { email = "  New.User@Example.com ", password = "a-good-password", displayName = " Nasim " }, Json);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("new.user@example.com", body.GetProperty("user").GetProperty("email").GetString()); // normalized
        Assert.Equal("Nasim", body.GetProperty("user").GetProperty("displayName").GetString());
        Assert.False(body.GetProperty("user").TryGetProperty("passwordHash", out _));

        var cookie = Accounts.RefreshCookieOf(response)!.ToLowerInvariant();
        Assert.Contains("httponly", cookie);
        Assert.Contains("samesite=strict", cookie);
        Assert.Contains("path=/api/auth", cookie);
    }

    [Fact]
    public async Task The_password_is_stored_hashed_and_the_refresh_token_only_as_a_hash()
    {
        var session = await Accounts.Register(factory, "plain-text-password");
        var (user, token) = await factory.WithDbAsync(async db =>
            (await db.Users.AsNoTracking().SingleAsync(u => u.Email == session.Email),
                await db.RefreshTokens.AsNoTracking().OrderByDescending(t => t.CreatedAt).FirstAsync()));

        Assert.DoesNotContain("plain-text-password", user.PasswordHash);
        Assert.NotEqual(session.RefreshCookie.Split('=')[1], token.TokenHash);
        Assert.Equal(64, token.TokenHash.Length);
    }

    [Fact]
    public async Task Me_needs_a_valid_access_token()
    {
        var session = await Accounts.Register(factory);

        var me = await session.Send(HttpMethod.Get, "/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Equal(session.Email, (await me.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("email").GetString());

        Assert.Equal(HttpStatusCode.Unauthorized, (await session.Client.GetAsync("/api/auth/me")).StatusCode);

        var tampered = session.AccessToken[..^3] + "abc";
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tampered);
        Assert.Equal(HttpStatusCode.Unauthorized, (await session.Client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task Register_rejects_a_duplicate_email_in_any_case_and_weak_input()
    {
        var session = await Accounts.Register(factory);
        var client = session.Client;

        var duplicate = await client.PostAsJsonAsync("/api/auth/register",
            new { email = session.Email.ToUpperInvariant(), password = "another-password", displayName = "Dup" }, Json);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        foreach (var bad in new object[]
                 {
                     new { email = "not-an-email", password = "long-enough-1", displayName = "Ok" },
                     new { email = "a@example.com", password = "short", displayName = "Ok" },
                     new { email = "a@example.com", password = "123456789", displayName = "Ok" },   // only digits
                     new { email = "a@example.com", password = "long-enough-1", displayName = "x" } // name too short
                 })
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/register", bad, Json)).StatusCode);
    }

    [Fact]
    public async Task Login_works_and_does_not_tell_a_wrong_password_from_an_unknown_email()
    {
        var session = await Accounts.Register(factory);

        var ok = await Accounts.Login(session.Client, session.Email.ToUpperInvariant(), session.Password);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.NotNull(Accounts.RefreshCookieOf(ok));

        var wrong = await Accounts.Login(session.Client, session.Email, "wrong-password");
        var unknown = await Accounts.Login(session.Client, "nobody@example.com", "wrong-password");
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        var wrongBody = await wrong.Content.ReadFromJsonAsync<JsonElement>(Json);
        var unknownBody = await unknown.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal(wrongBody.GetProperty("detail").GetString(), unknownBody.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account_even_for_the_right_password()
    {
        var session = await Accounts.Register(factory);
        for (var i = 0; i < 5; i++)
            Assert.Equal(HttpStatusCode.Unauthorized, (await Accounts.Login(session.Client, session.Email, "wrong-password")).StatusCode);

        Assert.Equal(HttpStatusCode.TooManyRequests, (await Accounts.Login(session.Client, session.Email, session.Password)).StatusCode);
    }

    [Fact]
    public async Task A_successful_login_resets_the_failure_count()
    {
        var session = await Accounts.Register(factory);
        for (var i = 0; i < 4; i++) await Accounts.Login(session.Client, session.Email, "wrong-password");
        Assert.Equal(HttpStatusCode.OK, (await Accounts.Login(session.Client, session.Email, session.Password)).StatusCode);
        for (var i = 0; i < 4; i++) await Accounts.Login(session.Client, session.Email, "wrong-password");
        Assert.Equal(HttpStatusCode.OK, (await Accounts.Login(session.Client, session.Email, session.Password)).StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_a_replayed_one_ends_every_session()
    {
        var session = await Accounts.Register(factory);

        var refreshed = await session.Client.SendAsync(RefreshRequest(session.RefreshCookie));
        Assert.Equal(HttpStatusCode.OK, refreshed.StatusCode);
        var newCookie = Accounts.CookieValue(Accounts.RefreshCookieOf(refreshed)!);
        Assert.NotEqual(session.RefreshCookie, newCookie);
        var newAccess = (await refreshed.Content.ReadFromJsonAsync<JsonElement>(Json)).GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrEmpty(newAccess));

        // the old refresh token is replayed (stolen?): refused, and the new one stops working too
        var replay = await session.Client.SendAsync(RefreshRequest(session.RefreshCookie));
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await session.Client.SendAsync(RefreshRequest(newCookie))).StatusCode);
    }

    [Fact]
    public async Task Refresh_without_a_cookie_or_with_a_made_up_one_is_unauthorized_and_clears_the_cookie()
    {
        var client = Accounts.NewClient(factory);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/auth/refresh", null)).StatusCode);

        var fake = await client.SendAsync(RefreshRequest("interviewpal_refresh=made-up-token"));
        Assert.Equal(HttpStatusCode.Unauthorized, fake.StatusCode);
        Assert.Contains("expires=", Accounts.RefreshCookieOf(fake)!.ToLowerInvariant()); // the cookie is deleted
    }

    [Fact]
    public async Task Logout_revokes_the_refresh_token()
    {
        var session = await Accounts.Register(factory);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Add("Cookie", session.RefreshCookie);
        Assert.Equal(HttpStatusCode.NoContent, (await session.Client.SendAsync(request)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await session.Client.SendAsync(RefreshRequest(session.RefreshCookie))).StatusCode);
    }

    [Fact]
    public async Task Change_password_needs_the_current_one_and_ends_every_session()
    {
        var session = await Accounts.Register(factory);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await session.Send(HttpMethod.Post, "/api/auth/change-password",
                new { currentPassword = "not-my-password", newPassword = "brand-new-password" })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent,
            (await session.Send(HttpMethod.Post, "/api/auth/change-password",
                new { currentPassword = session.Password, newPassword = "brand-new-password" })).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await session.Client.SendAsync(RefreshRequest(session.RefreshCookie))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Accounts.Login(session.Client, session.Email, session.Password)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Accounts.Login(session.Client, session.Email, "brand-new-password")).StatusCode);
    }

    [Fact]
    public async Task Delete_account_needs_the_password_and_removes_the_user_and_the_progress()
    {
        var session = await Accounts.Register(factory);
        var evaluated = await session.Send(HttpMethod.Post, "/api/practice/evaluate",
            new { answers = new[] { new { questionId = "sample-short-senior", knewIt = true } }, mode = "Learning" });
        Assert.Equal(HttpStatusCode.OK, evaluated.StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest,
            (await session.Send(HttpMethod.Post, "/api/auth/delete-account", new { password = "wrong" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await session.Send(HttpMethod.Post, "/api/auth/delete-account", new { password = session.Password })).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Accounts.Login(session.Client, session.Email, session.Password)).StatusCode);
        var leftovers = await factory.WithDbAsync(async db => await db.PracticeHistory.CountAsync(h => h.UserId.ToString() == session.Body.GetProperty("user").GetProperty("id").GetString()));
        Assert.Equal(0, leftovers);
    }

    [Fact]
    public async Task The_database_has_no_second_account_with_the_same_email()
    {
        var session = await Accounts.Register(factory);
        var count = await factory.WithDbAsync(db => db.Users.CountAsync(u => u.Email == session.Email));
        Assert.Equal(1, count);
    }
}

/// <summary>A tiny limit, to see the per-IP rate limit of the auth endpoints.</summary>
public class RateLimitFactory : ApiFactory
{
    protected override Dictionary<string, string?> Settings => new() { ["Auth:RequestsPerMinute"] = "3" };
}

public class AuthRateLimitTests(RateLimitFactory factory) : IClassFixture<RateLimitFactory>
{
    [Fact]
    public async Task Auth_endpoints_are_limited_per_address()
    {
        var client = factory.CreateClient();
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 5; i++)
            statuses.Add((await client.PostAsJsonAsync("/api/auth/login", new { email = "a@example.com", password = "x" })).StatusCode);

        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[^1]);
        Assert.Equal(HttpStatusCode.Unauthorized, statuses[0]);

        // other endpoints are not limited
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }
}
