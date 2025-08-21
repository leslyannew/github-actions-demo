using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Entities;
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? LastLoginTime { get; set; }
}

