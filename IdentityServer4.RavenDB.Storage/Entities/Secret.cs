using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.RavenDB.Entities
{
    public class Secret
    {
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime? Expiration { get; set; }
        public string Type { get; set; } = "SharedSecret";
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
