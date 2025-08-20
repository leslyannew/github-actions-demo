using System.Globalization;
using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Identity;

namespace github_actions_demo.Features.UserAdministration;
public static class CreateRole
{
    public record Command : IRequest<IdentityResult>
    {
        public required string Name { get; init; }
    }

    internal sealed class CommandValidator
    : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty()
                .WithMessage("The name of the role can't be empty.");
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
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            IdentityResult result = await roleManager.CreateAsync(new IdentityRole(ti.ToTitleCase(request.Name)));
            return result;
        }
    }
}

