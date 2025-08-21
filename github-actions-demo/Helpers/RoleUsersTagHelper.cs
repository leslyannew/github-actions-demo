using github_actions_demo.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace github_actions_demo.Helpers;
[HtmlTargetElement("td", Attributes = "role-id")]
public class RoleUsersTagHelper(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : TagHelper
{
    [HtmlAttributeName("role-id")]
    public required string RoleId { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        List<string> names = await GetUsersInRole(RoleId);
        string users = names.Count == 0 ? "No Users" : string.Join(", ", names);

        output.Content.SetHtmlContent(users);
    }

    public async Task<List<string>> GetUsersInRole(string roleId)
    {
        var names = new List<string>();
        IdentityRole? role = await roleManager.FindByIdAsync(roleId);
        if (role != null)
        {
            foreach (ApplicationUser user in userManager.Users)
            {
                if (user != null && await userManager.IsInRoleAsync(user, role.Name!))
                {
                    names.Add(user.UserName!);
                }
            }
        }
        return names;
    }
}

