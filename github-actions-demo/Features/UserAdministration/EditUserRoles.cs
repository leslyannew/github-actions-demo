using System.Security.Principal;
using AutoMapper;
using FluentValidation;
using github_actions_demo.Entities;
using github_actions_demo.Features.UserAdministration.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration;
public static class EditUserRoles
{
    public record Query : IRequest<UserRoles?>
    {
        public required string UserId { get; init; }
    }

    internal sealed class QueryValidator
   : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage("The UserId can't be empty.");
        }
    }

    public class QueryHandler
    : IRequestHandler<Query, UserRoles?>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;
        public QueryHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mapper = mapper;
        }

        public async Task<UserRoles?> Handle(
            Query request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                var memberRoles = new List<IdentityRole>();
                var nonMemberRoles = new List<IdentityRole>();
                foreach (IdentityRole role in roleManager.Roles)
                {
                    bool isInRole = await userManager.IsInRoleAsync(user, role.Name!);
                    if (isInRole)
                    {
                        memberRoles.Add(role);
                    }
                    else
                    {
                        nonMemberRoles.Add(role);
                    }
                }
                UserDto? userDto = mapper.Map<ApplicationUser, UserDto>(user);
                return new UserRoles
                {
                    User = userDto,
                    MemberRoles = memberRoles,
                    NonMemberRoles = nonMemberRoles
                };
            }
            else
            {
                return null;
            }
        }
    }

    public record Command : IRequest<IdentityResult>
    {
        public required string UserId { get; init; }
        public bool IsEnabled { get; init; }
        public string[]? AddIds { get; init; }
        public string[]? DeleteIds { get; init; }
    }
    public class CommandHandler
        : IRequestHandler<Command, IdentityResult>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<CommandHandler> logger;

        public CommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CommandHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> Handle(
            Command request, CancellationToken cancellationToken)
        {
            var result = new IdentityResult();
            IIdentity? me = httpContextAccessor!.HttpContext!.User.Identity;
            ApplicationUser user = await userManager.FindByIdAsync(request.UserId);

            if (user != null)
            {
                if (user.IsEnabled != request.IsEnabled)
                {
                    user.IsEnabled = request.IsEnabled;
                    result = await userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        logger.UserAdministrationError("enabling user", me!.Name!, string.Join(",", result.Errors));
                    }
                }
            }
            else
            {
                _ = result.Errors.Append(new IdentityError { Description = "No user found" });
            }

            foreach (string roleId in request.AddIds ?? [])
            {
                IdentityRole? role = await roleManager.FindByIdAsync(roleId);
                if (role != null && user != null)
                {
                    result = await userManager.AddToRoleAsync(user!, role.Name!);
                    if (!result.Succeeded)
                    {
                        logger.UserAdministrationError("updating a user membership", me!.Name!, string.Join(",", result.Errors));
                    }
                }
            }
            foreach (string roleId in request.DeleteIds ?? [])
            {
                IdentityRole? role = await roleManager.FindByIdAsync(roleId);

                if (role != null && user != null)
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name!);
                    if (!result.Succeeded)
                    {
                        logger.UserAdministrationError("deleting user membership", me!.Name!, string.Join(",", result.Errors));
                    }
                }
            }
            return result;
        }
    }
}
