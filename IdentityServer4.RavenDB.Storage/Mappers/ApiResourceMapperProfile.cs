using AutoMapper;

namespace IdentityServer4.RavenDB.Mappers
{
    public class ApiResourceMapperProfile : BaseMapperProfile
    {
        public ApiResourceMapperProfile() : base()
        {
            CreateMap<Entities.ApiResource, Models.ApiResource>(MemberList.Destination)
                .ConstructUsing(src => new Models.ApiResource())
                .ForMember(x => x.ApiSecrets, opts => opts.MapFrom(x => x.Secrets))
                .ReverseMap();

            CreateMap<Entities.Secret, Models.Secret>(MemberList.Destination)
                .ForMember(dest => dest.Type, opt => opt.Condition(srs => srs != null))
                .ReverseMap();
        }
    }
}
