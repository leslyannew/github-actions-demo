using System.ComponentModel.DataAnnotations;

namespace github_actions_demo.Features.UserAdministration.Dto;
public class RoleUsersEdit
{
    [Required]
    public required string RoleName { get; set; }

    public required string RoleId { get; set; }

    public string[]? AddIds { get; set; }

    public string[]? DeleteIds { get; set; }
}
