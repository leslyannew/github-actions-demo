using AutoMapper;
using FluentValidation;
using github_actions_demo.Entities;
using github_actions_demo.Features.UserAdministration.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration;
public static class GetRoleUsers
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
    public class Handler
    : IRequestHandler<Query, RoleUsers?>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;
        public Handler(
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
}


