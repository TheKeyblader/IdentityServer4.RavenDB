﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.RavenDB.Entities
{
    public class ClientClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
