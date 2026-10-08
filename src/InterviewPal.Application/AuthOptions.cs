namespace InterviewPal.Application;

/// <summary>Settings of the login system (section "Auth" of the configuration).</summary>
public class AuthOptions
{
    public const string Section = "Auth";

    /// <summary>Secret used to sign access tokens. At least 32 characters; required outside Development.</summary>
    public string JwtKey { get; set; } = "";

    public string Issuer { get; set; } = "interviewpal";
    public string Audience { get; set; } = "interviewpal-app";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;

    /// <summary>Wrong passwords for one account before it is locked for <see cref="LockoutMinutes"/>.</summary>
    public int MaxFailedLogins { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;

    /// <summary>Requests per minute and IP address to the login, register and refresh endpoints.</summary>
    public int RequestsPerMinute { get; set; } = 30;
}
