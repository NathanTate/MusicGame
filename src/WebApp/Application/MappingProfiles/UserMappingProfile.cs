﻿using Application.DTO.Users;
using AutoMapper;
using Domain.Entities;

namespace Application.MappingProfiles;
internal class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

        CreateMap<User, ArtistResponse>();
    }
}