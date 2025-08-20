using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration.Dto;
public class RoleUsers
{
    public required IdentityRole Role { get; set; }
    public required IEnumerable<UserDto> Members { get; set; }
    public required IEnumerable<UserDto> NonMembers { get; set; }
}
