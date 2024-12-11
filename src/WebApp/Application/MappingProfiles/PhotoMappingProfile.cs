using Application.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class PhotoMappingProfile : Profile
{
    public PhotoMappingProfile()
    {
        CreateMap<Photo, PhotoResponse>();
        CreateMap<Photo?, PhotoResponse?>();
    }
}
