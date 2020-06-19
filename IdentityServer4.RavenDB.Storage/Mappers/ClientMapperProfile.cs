namespace IdentityServer4.RavenDB.Mappers
{
    public class ClientMapperProfile : BaseMapperProfile
    {
        public ClientMapperProfile() : base()
        {
            CreateMap<Entities.Client, Models.Client>()
                .ForMember(dest => dest.ProtocolType, opt => opt.Condition(srs => srs != null))
                .ReverseMap();
        }
    }
}
