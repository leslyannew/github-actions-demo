using System.ComponentModel.DataAnnotations;

namespace github_actions_demo.Features.UserAdministration.Dto;
public class UserRolesEdit
{
    [Required]
    public required string UserId { get; set; }
    public bool IsEnabled { get; set; }
    public string[]? AddIds { get; set; }
    public string[]? DeleteIds { get; set; }
}
