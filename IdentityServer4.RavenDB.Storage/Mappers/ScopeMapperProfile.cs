using AutoMapper;

namespace IdentityServer4.RavenDB.Mappers
{
    /// <summary>
    /// Defines entity/model mapping for scopes.
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class ScopeMapperProfile : BaseMapperProfile
    {
        /// <summary>
        /// <see cref="ScopeMapperProfile"/>
        /// </summary>
        public ScopeMapperProfile() : base()
        {
            CreateMap<Entities.ApiScope, Models.ApiScope>(MemberList.Destination)
                .ConstructUsing(src => new Models.ApiScope())
                .ForMember(x => x.Properties, opts => opts.MapFrom(x => x.Properties))
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(x => x.UserClaims))
                .ReverseMap();
        }
    }
}
