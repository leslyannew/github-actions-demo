using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration.Dto;
public class UserRoles
{
    public required UserDto User { get; set; }
    public required IEnumerable<IdentityRole> MemberRoles { get; set; }
    public required IEnumerable<IdentityRole> NonMemberRoles { get; set; }
}
