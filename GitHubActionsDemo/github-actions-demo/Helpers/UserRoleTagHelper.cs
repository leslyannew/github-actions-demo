using github_actions_demo.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace github_actions_demo.Helpers;
[HtmlTargetElement("td", Attributes = "user-id")]
public class UserRolesTagHelper(
        UserManager<ApplicationUser> userManager) : TagHelper
{
    [HtmlAttributeName("user-id")]
    public required string UserId { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        List<string> names = await GetRolesForUser(UserId);
        string roles = names.Count == 0 ? "No Roles" : string.Join(", ", names);

        output.Content.SetHtmlContent(roles);
    }

    public async Task<List<string>> GetRolesForUser(string userId)
    {
        var roles = new List<string>();
        ApplicationUser? user = await userManager.FindByIdAsync(userId);
        IList<string> roleNames = await userManager.GetRolesAsync(user!);
        if (roleNames != null)
        {
            foreach (string role in roleNames)
            {

                roles.Add(role);
            }
        }
        return roles;
    }
}

