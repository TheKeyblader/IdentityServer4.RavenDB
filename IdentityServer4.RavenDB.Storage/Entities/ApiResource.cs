using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.RavenDB.Entities
{
    public class ApiResource
    {
        public string Id { get; set; }
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> AllowedAccessTokenSigningAlgorithms { get; set; }
        public bool ShowInDiscoveryDocument { get; set; } = true;
        public List<Secret> Secrets { get; set; }
        public List<string> Scopes { get; set; }
        public List<string> Claims { get; set; }
        public List<Property> Properties { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Updated { get; set; }
        public DateTime? LastAccessed { get; set; }
        public bool NonEditable { get; set; }
    }
}
