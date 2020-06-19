using AutoMapper;

namespace IdentityServer4.RavenDB.Mappers
{
    public class IdentityResourceMapperProfile : BaseMapperProfile
    {
        public IdentityResourceMapperProfile() : base()
        {
            CreateMap<Entities.IdentityResource, Models.IdentityResource>(MemberList.Destination)
                .ConstructUsing(src => new Models.IdentityResource())
                .ReverseMap();
        }
    }
}
