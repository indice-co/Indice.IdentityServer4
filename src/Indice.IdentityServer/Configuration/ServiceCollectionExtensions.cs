using IdentityServer4.EntityFramework;
using IdentityServer4.Stores;
using Indice.IdentityServer.EntityFramework;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

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
        var cleanup = builder.Services.Where(x => x.ImplementationType == typeof(TokenCleanupHost)).FirstOrDefault();
        if (cleanup is not null) {
            builder.Services.Remove(cleanup);
        }
        var cleanupService = builder.Services.Where(x => x.ImplementationType == typeof(TokenCleanupService)).FirstOrDefault();
        if (cleanupService is not null) {
            builder.Services.Remove(cleanupService);
        }
        builder.Services.AddTransient<ExtendedTokenCleanupService>();
        builder.Services.AddSingleton<IHostedService, ExtendedTokenCleanupHost>();
        return builder;
    }
}