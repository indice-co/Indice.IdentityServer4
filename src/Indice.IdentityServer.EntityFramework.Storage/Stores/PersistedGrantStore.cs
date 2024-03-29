// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.Interfaces;
using Indice.IdentityServer.EntityFramework.Storage.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Extensions;
using IS4Entities = IdentityServer4.EntityFramework.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Indice.IdentityServer.EntityFramework.Storage.Stores;

/// <summary>
/// Implementation of IPersistedGrantStore thats uses EF.
/// </summary>
/// <seealso cref="IPersistedGrantStore" />
public class PersistedGrantStore : IPersistedGrantStore
{
    /// <summary>
    /// The DbContext.
    /// </summary>
    protected readonly IPersistedGrantDbContext Context;

    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersistedGrantStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>
    public PersistedGrantStore(IPersistedGrantDbContext context, ILogger<PersistedGrantStore> logger) {
        Context = context;
        Logger = logger;
    }

    /// <inheritdoc/>
    public virtual async Task StoreAsync(PersistedGrant token) {
        var existing = (await Context.PersistedGrants.Where(x => x.Key == token.Key).ToArrayAsync())
            .SingleOrDefault(x => x.Key == token.Key);
        if (existing == null) {
            Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

            var persistedGrant = token.ToEntity();
            Context.PersistedGrants.Add(persistedGrant);
        } else {
            Logger.LogDebug("{persistedGrantKey} found in database", token.Key);

            token.UpdateEntity(existing);
        }

        try {
            await Context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException ex) {
            Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<PersistedGrant> GetAsync(string key) {
        var persistedGrant = (await Context.PersistedGrants.AsNoTracking().Where(x => x.Key == key).ToArrayAsync())
            .SingleOrDefault(x => x.Key == key);
        var model = persistedGrant?.ToModel();

        Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

        return model;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter) {
        filter.Validate();

        var persistedGrants = await Filter(Context.PersistedGrants.AsQueryable(), filter).ToArrayAsync();
        persistedGrants = Filter(persistedGrants.AsQueryable(), filter).ToArray();

        var model = persistedGrants.Select(x => x.ToModel());

        Logger.LogDebug("{persistedGrantCount} persisted grants found for {@filter}", persistedGrants.Length, filter);

        return model;
    }

    /// <inheritdoc/>
    public virtual async Task RemoveAsync(string key) {
        var persistedGrant = (await Context.PersistedGrants.Where(x => x.Key == key).ToArrayAsync())
            .SingleOrDefault(x => x.Key == key);
        if (persistedGrant != null) {
            Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

            Context.PersistedGrants.Remove(persistedGrant);

            try {
                await Context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException ex) {
                Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
            }
        } else {
            Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAllAsync(PersistedGrantFilter filter) {
        filter.Validate();
        var count = await Filter(Context.PersistedGrants.AsQueryable(), filter).ExecuteDeleteAsync();
        Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for {@filter}", count, filter);
    }


    private IQueryable<IS4Entities.PersistedGrant> Filter(IQueryable<IS4Entities.PersistedGrant> query, PersistedGrantFilter filter) {
        if (!string.IsNullOrWhiteSpace(filter.ClientId)) {
            query = query.Where(x => x.ClientId == filter.ClientId);
        }
        if (!string.IsNullOrWhiteSpace(filter.SessionId)) {
            query = query.Where(x => x.SessionId == filter.SessionId);
        }
        if (!string.IsNullOrWhiteSpace(filter.SubjectId)) {
            query = query.Where(x => x.SubjectId == filter.SubjectId);
        }
        if (!string.IsNullOrWhiteSpace(filter.Type)) {
            query = query.Where(x => x.Type == filter.Type);
        }

        return query;
    }
}