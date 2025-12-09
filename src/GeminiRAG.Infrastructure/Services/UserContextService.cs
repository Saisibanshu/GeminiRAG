using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GeminiRAG.Infrastructure.Services;

public interface IUserContextService
{
    Guid GetCurrentUserId();
    string GetCurrentUserEmail();
}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        return userId;
    }

    public string GetCurrentUserEmail()
    {
        var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("User email not found");
        }

        return email;
    }
}
