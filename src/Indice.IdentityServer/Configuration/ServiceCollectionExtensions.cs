using IdentityServer4.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions on <see cref="IIdentityServerBuilder"/>
    /// </summary>
    public static class IndiceIdentityServer4BuilderExtensions
    {
        /// <summary>
        /// Adds support for dotnet7 to the existing Stores.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDotnet7CompatibleStores(this IIdentityServerBuilder builder) {
            builder.AddClientStore<Indice.IdentityServer.EntityFramework.Storage.Stores.ClientStore>();
            builder.AddResourceStore<Indice.IdentityServer.EntityFramework.Storage.Stores.ResourceStore>();
            builder.Services.AddTransient<IPersistedGrantStore, Indice.IdentityServer.EntityFramework.Storage.Stores.PersistedGrantStore>();
            return builder;
        }
    }
}