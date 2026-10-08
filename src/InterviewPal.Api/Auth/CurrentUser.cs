using System.Security.Claims;
using InterviewPal.Application.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace InterviewPal.Api.Auth;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? Id =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;
}
