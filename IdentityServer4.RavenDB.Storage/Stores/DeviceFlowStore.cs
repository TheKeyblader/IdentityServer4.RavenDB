using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.RavenDB.Options;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;

namespace IdentityServer4.RavenDB.Stores
{
    public class DeviceFlowStore : IDeviceFlowStore
    {
        protected readonly IAsyncDocumentSession Session;
        protected readonly IPersistentGrantSerializer Serializer;
        protected readonly ILogger<DeviceFlowStore> Logger;
        protected readonly OperationalStoreOptions Options;

        public DeviceFlowStore(IAsyncDocumentSession session, IPersistentGrantSerializer serializer,
                               ILogger<DeviceFlowStore> logger, OperationalStoreOptions options)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Serializer = serializer;
            Logger = logger;
            Options = options;
        }

        public virtual async Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = await Session.Query<Entities.DeviceFlowCodes>().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);
            var model = ToModel(deviceFlowCodes?.Data);

            Logger.LogDebug("{deviceCode} found in database: {deviceCodeFound}", deviceCode, model != null);

            return model;
        }

        public virtual async Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            var deviceFlowCodes = await Session.Query<Entities.DeviceFlowCodes>().FirstOrDefaultAsync(x => x.UserCode == userCode);
            var model = ToModel(deviceFlowCodes?.Data);

            Logger.LogDebug("{userCode} found in database: {userCodeFound}", userCode, model != null);

            return model;
        }

        public virtual async Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = await Session.Query<Entities.DeviceFlowCodes>().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);

            if (deviceFlowCodes != null)
            {
                Logger.LogDebug("removing {deviceCode} device code from database", deviceCode);

                Session.Delete(deviceFlowCodes);

                try
                {
                    await Session.SaveChangesAsync();
                }
                catch (ConcurrencyException ex)
                {
                    Logger.LogInformation("exception removing {deviceCode} device code from database: {error}", deviceCode, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {deviceCode} device code found in database", deviceCode);
            }
        }

        public virtual async Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data)
        {
            var entity = ToEntity(data, deviceCode, userCode);
            await Session.StoreAsync(entity);

            if (Options.SetTokenExpire && entity.Expiration.HasValue)
            {
                Session.Advanced.GetMetadataFor(entity)[Constants.Documents.Metadata.Expires] = entity.Expiration.Value.ToUniversalTime();
            }

            await Session.SaveChangesAsync();
        }

        public virtual async Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            var existing = await Session.Query<Entities.DeviceFlowCodes>().SingleOrDefaultAsync(x => x.UserCode == userCode);
            if (existing == null)
            {
                Logger.LogError("{userCode} not found in database", userCode);
                throw new InvalidOperationException("Could not update device code");
            }

            var entity = ToEntity(data, existing.DeviceCode, userCode);
            Logger.LogDebug("{userCode} found in database", userCode);

            existing.SubjectId = data.Subject?.FindFirst(JwtClaimTypes.Subject).Value;
            existing.Data = entity.Data;

            try
            {
                await Session.SaveChangesAsync();
            }
            catch (ConcurrencyException ex)
            {
                Logger.LogWarning("exception updating {userCode} user code in database: {error}", userCode, ex.Message);
            }
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="deviceCode"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        protected Entities.DeviceFlowCodes ToEntity(DeviceCode model, string deviceCode, string userCode)
        {
            if (model == null || deviceCode == null || userCode == null) return null;

            return new Entities.DeviceFlowCodes
            {
                DeviceCode = deviceCode,
                UserCode = userCode,
                ClientId = model.ClientId,
                SubjectId = model.Subject?.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = model.CreationTime,
                Expiration = model.CreationTime.AddSeconds(model.Lifetime),
                Data = Serializer.Serialize(model)
            };
        }

        /// <summary>
        /// Converts a serialized DeviceCode to a model.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected DeviceCode ToModel(string entity)
        {
            if (entity == null) return null;

            return Serializer.Deserialize<DeviceCode>(entity);
        }
    }
}
