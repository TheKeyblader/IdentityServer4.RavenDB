using System.Collections.Generic;
using AutoMapper;

namespace IdentityServer4.RavenDB.Mappers
{
    public abstract class BaseMapperProfile : Profile
    {
        protected BaseMapperProfile()
        {
            CreateMap<Entities.Property, KeyValuePair<string, string>>()
                .ReverseMap();

            CreateMap<Entities.Secret, Models.Secret>(MemberList.Destination)
                .ForMember(dest => dest.Type, opt => opt.Condition(srs => srs != null))
                .ReverseMap();
        }
    }
}
