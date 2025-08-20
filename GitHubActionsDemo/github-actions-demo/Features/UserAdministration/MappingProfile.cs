using AutoMapper;

using github_actions_demo.Entities;
using github_actions_demo.Features.UserAdministration.Dto;

namespace github_actions_demo.Features.UserAdministration;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ReverseMap();
    }
}
