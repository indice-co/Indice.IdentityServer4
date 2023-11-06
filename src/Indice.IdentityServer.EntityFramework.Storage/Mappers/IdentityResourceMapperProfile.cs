// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AutoMapper;
using IS4Models = IdentityServer4.Models;
using IS4Entities = IdentityServer4.EntityFramework.Entities;

namespace Indice.IdentityServer.EntityFramework.Storage.Mappers;

/// <summary>
/// Defines entity/model mapping for identity resources.
/// </summary>
/// <seealso cref="AutoMapper.Profile" />
public class IdentityResourceMapperProfile : Profile
{
    /// <summary>
    /// <see cref="IdentityResourceMapperProfile"/>
    /// </summary>
    public IdentityResourceMapperProfile() {
        CreateMap<IS4Entities.IdentityResourceProperty, KeyValuePair<string, string>>()
            .ReverseMap();

        CreateMap<IS4Entities.IdentityResource, IS4Models.IdentityResource>(MemberList.Destination)
            .ConstructUsing(src => new IS4Models.IdentityResource())
            .ReverseMap();

        CreateMap<IS4Entities.IdentityResourceClaim, string>()
           .ConstructUsing(x => x.Type)
           .ReverseMap()
           .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src));
    }
}
