using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinTracker.Web.Services;

public sealed class CurrentUserService(AuthenticationStateProvider authStateProvider)
{
    public async Task<string?> GetUserIdAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
