namespace InterviewPal.Api.Auth;

/// <summary>
/// The refresh token lives in an httpOnly cookie, so scripts (and therefore XSS) cannot read it. SameSite=Strict
/// plus a path limited to /api/auth means the browser sends it only to the auth endpoints, from our own site.
/// </summary>
public static class RefreshCookie
{
    public const string Name = "interviewpal_refresh";
    private const string Path = "/api/auth";

    public static void Set(HttpResponse response, string token, DateTime expiresAt) =>
        response.Cookies.Append(Name, token, Options(response.HttpContext, expiresAt));

    public static void Clear(HttpResponse response) =>
        response.Cookies.Delete(Name, Options(response.HttpContext, null));

    public static string? Read(HttpRequest request) => request.Cookies[Name];

    private static CookieOptions Options(HttpContext context, DateTime? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = context.Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAt is null ? null : new DateTimeOffset(expiresAt.Value, TimeSpan.Zero),
        IsEssential = true
    };
}
