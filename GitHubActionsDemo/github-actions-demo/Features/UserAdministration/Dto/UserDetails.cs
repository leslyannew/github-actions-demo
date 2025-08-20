using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration.Dto;
public class UserDetails
{
    public required UserDto User { get; set; }
    public required IEnumerable<string> MemberRoles { get; set; }
}
