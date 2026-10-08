using System.Net;
using System.Threading.RateLimiting;
using InterviewPal.Application;
using InterviewPal.Application.Abstractions;
using InterviewPal.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace InterviewPal.Api.Auth;

public static class AuthExtensions
{
    public const string RateLimitPolicy = "auth";

    /// <summary>
    /// JWT bearer authentication, the options behind it and the per-IP rate limit of the auth endpoints.
    /// Everything reads the configuration lazily so test and host overrides apply.
    /// </summary>
    public static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddOptions<AuthOptions>()
            .BindConfiguration(AuthOptions.Section)
            .Validate(o => o.JwtKey.Length >= 32, "Auth:JwtKey must be at least 32 characters (set the Auth__JwtKey environment variable).")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthOptions>>((jwt, auth) =>
            {
                var o = auth.Value;
                jwt.MapInboundClaims = false; // keep "sub" as "sub"
                jwt.TokenValidationParameters = new()
                {
                    ValidIssuer = o.Issuer,
                    ValidAudience = o.Audience,
                    IssuerSigningKey = TokenService.SigningKey(o),
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name"
                };
            });
        services.AddAuthorization();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.AddPolicy(RateLimitPolicy, context =>
            {
                var requests = context.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value.RequestsPerMinute;
                return RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = requests, Window = TimeSpan.FromMinutes(1) });
            });
        });

        // Behind Render (and Vercel in front of it) the client address and https come from X-Forwarded-* headers.
        services.Configure<ForwardedHeadersOptions>(o =>
        {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            o.ForwardLimit = 2; // Render's proxy and Vercel's
            o.KnownIPNetworks.Clear();
            o.KnownProxies.Clear();
        });
        return services;
    }
}
