using System.Security.Principal;
using AutoMapper;
using FluentValidation;
using github_actions_demo.Entities;
using github_actions_demo.Features.UserAdministration.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration;
public static class EditRoleUsers
{
    public record Query : IRequest<RoleUsers?>
    {
        public required string RoleId { get; set; }
    }
    internal sealed class QueryValidator
   : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.RoleId)
                .NotEmpty()
                .WithMessage("The RoleId can't be empty.");
        }
    }

    public class QueryHandler
    : IRequestHandler<Query, RoleUsers?>
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

        public async Task<RoleUsers?> Handle(
            Query request, CancellationToken cancellationToken)
        {
            IdentityRole? role = await roleManager.FindByIdAsync(request.RoleId);

            if (role != null)
            {
                var members = new List<UserDto>();
                var nonMembers = new List<UserDto>();
                foreach (ApplicationUser user in userManager.Users)
                {
                    UserDto userDto = mapper.Map<ApplicationUser, UserDto>(user);
                    bool isInRole = await userManager.IsInRoleAsync(user, role.Name!);

                    if (isInRole)
                    {
                        members.Add(userDto);
                    }
                    else
                    {
                        nonMembers.Add(userDto);
                    }
                }
                return new RoleUsers
                {
                    Role = role!,
                    Members = members,
                    NonMembers = nonMembers
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
        public required string RoleName { get; init; }

        public required string RoleId { get; init; }

        public string[]? AddIds { get; init; }

        public string[]? DeleteIds { get; init; }
    }

    public class CommandHandler
    : IRequestHandler<Command, IdentityResult>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<UserAdministrationController> logger;

        public CommandHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<UserAdministrationController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityResult> Handle(
            Command request, CancellationToken cancellationToken)
        {
            var result = new IdentityResult();
            IIdentity? me = httpContextAccessor!.HttpContext!.User.Identity;
            foreach (string userId in request.AddIds ?? Array.Empty<string>())
            {
                ApplicationUser? user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    result = await userManager.AddToRoleAsync(user, request.RoleName);
                    if (!result.Succeeded)
                    {
                        logger.UserAdministrationError("adding users to roles", me!.Name!, string.Join(",", result.Errors));
                    }
                }
            }
            foreach (string userId in request.DeleteIds ?? Array.Empty<string>())
            {
                ApplicationUser? user = await userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    result = await userManager.RemoveFromRoleAsync(user, request.RoleName);
                    if (!result.Succeeded)
                    {
                        logger.UserAdministrationError("removing users from roles", me!.Name!, string.Join(",", result.Errors));
                    }
                }
            }
            return result;
        }
    }
}
