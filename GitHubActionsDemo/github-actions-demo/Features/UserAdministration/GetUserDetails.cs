using AutoMapper;
using FluentValidation;
using github_actions_demo.Entities;
using github_actions_demo.Features.UserAdministration.Dto;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration;
public static class GetUserDetails
{
    public record Query : IRequest<UserDetails?>
    {
        public Guid Id { get; init; }
    }

    internal sealed class QueryValidator
  : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage("The UserId can't be empty.");
        }
    }
    public class Handler
    : IRequestHandler<Query, UserDetails?>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        public Handler(
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<UserDetails?> Handle(
            Query request, CancellationToken cancellationToken)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(request.Id.ToString());

            if (user != null)
            {
                IList<string> roles = await userManager.GetRolesAsync(user);

                UserDto userDto = mapper.Map<ApplicationUser, UserDto>(user);

                return new UserDetails
                {
                    User = userDto,
                    MemberRoles = roles
                };
            }
            else
            {
                throw new Exception();
            }
        }
    }
}


