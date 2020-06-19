﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.RavenDB.Mappers;
using IdentityServer4.RavenDB.Options;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;

namespace IdentityServer4.RavenDB.Stores
{
    public class PersistedGrantStore : IPersistedGrantStore
    {
        protected readonly IAsyncDocumentSession Session;
        protected readonly ILogger<PersistedGrantStore> Logger;
        protected readonly OperationalStoreOptions Options;

        public PersistedGrantStore(IAsyncDocumentSession session, ILogger<PersistedGrantStore> logger, OperationalStoreOptions options)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Logger = logger;
            Options = options;
        }

        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var persistedGrants = await Filter(filter).ToArrayAsync();
            var model = persistedGrants.Select(x => x.ToModel());

            Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);

            return model;
        }

        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = await Session.Query<Entities.PersistedGrant>().FirstOrDefaultAsync(x => x.Key == key);
            var model = persistedGrant?.ToModel();

            Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return model;
        }

        public virtual async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var persistedGrants = await Filter(filter).ToArrayAsync();

            Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", persistedGrants.Length, filter);

            Session.Delete(persistedGrants);

            try
            {
                await Session.SaveChangesAsync();
            }
            catch (ConcurrencyException ex)
            {
                Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {@filter}: {error}", persistedGrants.Length, filter, ex.Message);
            }
        }

        public virtual async Task RemoveAsync(string key)
        {
            var persistedGrant = await Session.Query<Entities.PersistedGrant>().FirstOrDefaultAsync(x => x.Key == key);
            if (persistedGrant != null)
            {
                Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

                Session.Delete(persistedGrant);

                try
                {
                    await Session.SaveChangesAsync();
                }
                catch (ConcurrencyException ex)
                {
                    Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
            }
        }

        public virtual async Task StoreAsync(PersistedGrant token)
        {
            var existing = await Session.Query<Entities.PersistedGrant>().SingleOrDefaultAsync(x => x.Key == token.Key);
            if (existing == null)
            {
                Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                var persistedGrant = token.ToEntity();
                await Session.StoreAsync(persistedGrant);
            }
            else
            {
                Logger.LogDebug("{persistedGrantKey} found in database", token.Key);

                token.UpdateEntity(existing);
            }

            if (Options.SetTokenExpire && existing.Expiration.HasValue)
            {
                Session.Advanced.GetMetadataFor(existing)[Constants.Documents.Metadata.Expires] = existing.Expiration.Value.ToUniversalTime();
            }

            try
            {
                await Session.SaveChangesAsync();
            }
            catch (ConcurrencyException ex)
            {
                Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
            }
        }

        private IRavenQueryable<Entities.PersistedGrant> Filter(PersistedGrantFilter filter)
        {
            var query = Session.Query<Entities.PersistedGrant>();

            if (!string.IsNullOrWhiteSpace(filter.ClientId))
            {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            return query;
        }
    }
}
