using Microsoft.AspNetCore.Identity;

namespace Sailabloom.Infrastructure.Data;

/// <summary>ASP.NET Core Identity user. Admin access is the "Admin" role.</summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
