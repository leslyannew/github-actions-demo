using System.ComponentModel.DataAnnotations;
using AutoMapper;
using github_actions_demo.Entities;
using github_actions_demo.Features.UserAdministration.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace github_actions_demo.Features.UserAdministration;
public class UserAdministrationController(
  RoleManager<IdentityRole> roleManager,
  UserManager<ApplicationUser> userManager,
  IMapper mapper,
  ILogger<UserAdministrationController> logger,
  ISender mediator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    #region User
    [HttpGet]
    public IActionResult Users()
    {
        IEnumerable<UserDto> usersDto = mapper.Map<IEnumerable<ApplicationUser>,
            IEnumerable<UserDto>>(userManager.Users);
        return View(usersDto);
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(Guid id)
    {
        if (ModelState.IsValid)
        {
            try
            {
                UserDetails? userDetails = await mediator.Send(new GetUserDetails.Query { Id = id });

                return View(userDetails);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "No user found");
            }
        }
        return View("Users", userManager.Users);
    }
    #endregion

    #region Role
    [HttpGet]
    public IActionResult Roles()
    {
        return View(roleManager.Roles);
    }

    [HttpGet]
    public IActionResult RoleCreate()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleCreate([Required] string name)
    {
        if (ModelState.IsValid)
        {
            IdentityResult result = await mediator.Send(new CreateRole.Command { Name = name.Replace(Environment.NewLine, "") });

            if (result.Succeeded)
            {
                logger.RoleCreated(User?.Identity?.Name!, name.Replace(Environment.NewLine, ""));
                return RedirectToAction("Roles");
            }
            else
            {
                logger.UserAdministrationError("creating role", User?.Identity?.Name!, string.Join(",", result.Errors));
                Errors(result);
            }
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleDelete([Required] string id)
    {
        if (ModelState.IsValid)
        {
            try
            {
                IdentityRole? role = await roleManager.FindByIdAsync(id);
                IdentityResult result = await mediator.Send(new DeleteRole.Command { Id = id });
                if (result.Succeeded)
                {
                    logger.RoleDeleted(User?.Identity?.Name!, role?.Name!);
                    return RedirectToAction("Roles");
                }
                else
                {
                    logger.UserAdministrationError("deleting role", User?.Identity?.Name!, string.Join(",", result.Errors));
                    Errors(result);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "No role found");
            }
        }
        return View("Roles", roleManager.Roles);
    }
    #endregion

    #region RoleUsers

    [HttpGet]
    public async Task<IActionResult> RoleUsersById([Required] string id)
    {
        if (ModelState.IsValid)
        {
            RoleUsers? roleUsers = await mediator.Send(new GetRoleUsers.Query { RoleId = id });
            if (roleUsers != null)
            {
                return View("RoleUsersDetails", roleUsers);
            }
            else
            {
                ModelState.AddModelError("", "No role found");
            }
        }
        return View("Roles", roleManager.Roles);
    }


    [HttpGet]
    public async Task<IActionResult> RoleUsersUpdate([Required] string id)
    {
        if (ModelState.IsValid)
        {
            RoleUsers? roleUsers = await mediator.Send(new EditRoleUsers.Query { RoleId = id });
            if (roleUsers != null)
            {
                return View(roleUsers);
            }
            else
            {
                ModelState.AddModelError("", "No role found");
            }
        }
        return View("Roles", roleManager.Roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleUsersUpdate(RoleUsersEdit model)
    {
        if (ModelState.IsValid)
        {
            IdentityResult result = await mediator.Send(new EditRoleUsers.Command
            {
                RoleName = model.RoleName,
                RoleId = model.RoleId,
                AddIds = model.AddIds,
                DeleteIds = model.DeleteIds
            });
            Errors(result);
            return RedirectToAction(nameof(Roles));
        }
        else
        {
            return await RoleUsersUpdate(model.RoleId);
        }
    }

    #endregion

    #region UserRoles 

    [HttpGet]
    public async Task<IActionResult> UserRolesUpdate([Required] string id)
    {
        if (ModelState.IsValid)
        {
            UserRoles? userRoles = await mediator.Send(new EditUserRoles.Query { UserId = id });
            if (userRoles != null)
            {
                return View(userRoles);
            }
            else
            {
                ModelState.AddModelError("", "No user found");
            }
        }
        return View("Users", userManager.Users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserRolesUpdate(UserRolesEdit model)
    {
        if (ModelState.IsValid)
        {
            IdentityResult result = await mediator.Send(new EditUserRoles.Command
            {
                UserId = model.UserId,
                IsEnabled = model.IsEnabled,
                AddIds = model.AddIds,
                DeleteIds = model.DeleteIds
            });
            Errors(result);
            return RedirectToAction(nameof(Users));
        }
        else
        {
            return await UserRolesUpdate(model.UserId);
        }
    }
    #endregion

    private void Errors(IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }
}

