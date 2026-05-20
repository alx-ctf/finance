using FinTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinTracker.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public UserProfile? Profile { get; set; }
}
