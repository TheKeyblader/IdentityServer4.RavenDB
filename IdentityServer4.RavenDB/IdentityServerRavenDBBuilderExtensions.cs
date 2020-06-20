using System;
using System.Collections.Generic;
using System.Text;
using IdentityServer4.RavenDB.Options;
using IdentityServer4.RavenDB.Services;
using IdentityServer4.RavenDB.Stores;
using IdentityServer4.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerRavenDBBuilderExtensions
    {
        /// <summary>
        /// Configures RavenDB implementation of IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStore(this IIdentityServerBuilder builder)
        {
            builder.AddClientStore<ClientStore>();
            builder.AddResourceStore<ResourceStore>();
            builder.AddCorsPolicyService<CorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Configures caching for IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStoreCache(this IIdentityServerBuilder builder)
        {
            builder.AddInMemoryCaching();

            // add the caching decorators
            builder.AddClientStoreCache<ClientStore>();
            builder.AddResourceStoreCache<ResourceStore>();
            builder.AddCorsPolicyCache<CorsPolicyService>();

            return builder;
        }

        /// <summary>
        /// Configures RavenDB implementation of IPersistedGrantStore with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStore(
            this IIdentityServerBuilder builder,
            Action<OperationalStoreOptions> storeOptionsAction = null)
        {
            var storeOptions = new OperationalStoreOptions();
            builder.Services.AddSingleton(storeOptions);
            storeOptionsAction?.Invoke(storeOptions);

            builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
            builder.Services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();
            return builder;
        }
    }
}
