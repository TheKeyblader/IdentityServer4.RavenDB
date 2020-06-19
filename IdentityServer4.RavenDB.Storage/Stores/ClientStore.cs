using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.RavenDB.Mappers;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents.Session;
using Entities = IdentityServer4.RavenDB.Entities;

namespace IdentityServer4.RavenDB.Stores
{
    public class ClientStore : IClientStore
    {
        protected readonly IAsyncDocumentSession Session;
        protected readonly ILogger<ClientStore> Logger;

        public ClientStore(IAsyncDocumentSession session,ILogger<ClientStore> logger)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Logger = logger;
        }

        public virtual async Task<Client> FindClientByIdAsync(string clientId)
        {
            var client = await Session.LoadAsync<Entities.Client>(clientId);
            if (client == null) return null;

            var model = client.ToModel();

            Logger.LogDebug("{clientId} found in database: {clientIdFound}", clientId, model != null);

            return model;
        }
    }
}
