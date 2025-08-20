using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration;
public static class DeleteRole
{
    public class Command : IRequest<IdentityResult>
    {
        public required string Id { get; set; }
    }

    internal sealed class CommandValidator
   : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.Id)
                .NotEmpty()
                .WithMessage("The Id of the role can't be empty.");
        }
    }

    public class Handler
    : IRequestHandler<Command, IdentityResult>
    {
        private readonly RoleManager<IdentityRole> roleManager;
        public Handler(
            RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        public async Task<IdentityResult> Handle(
            Command request, CancellationToken cancellationToken)
        {
            IdentityRole? role = await roleManager.FindByIdAsync(request.Id);

            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                return result;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}

