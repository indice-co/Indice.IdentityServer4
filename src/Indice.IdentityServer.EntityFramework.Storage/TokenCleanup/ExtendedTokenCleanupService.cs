// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Indice.IdentityServer.EntityFramework;

/// <summary>
/// Helper to cleanup stale persisted grants and device codes.
/// </summary>
public class ExtendedTokenCleanupService
{
    private readonly OperationalStoreOptions _options;
    private readonly IPersistedGrantDbContext _persistedGrantDbContext;
    private readonly IOperationalStoreNotification _operationalStoreNotification;
    private readonly ILogger<ExtendedTokenCleanupService> _logger;

    /// <summary>
    /// Constructor for TokenCleanupService.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="persistedGrantDbContext"></param>
    /// <param name="operationalStoreNotification"></param>
    /// <param name="logger"></param>
    public ExtendedTokenCleanupService(
        OperationalStoreOptions options,
        IPersistedGrantDbContext persistedGrantDbContext,
        ILogger<ExtendedTokenCleanupService> logger,
        IOperationalStoreNotification operationalStoreNotification = null) {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (_options.TokenCleanupBatchSize < 1) throw new ArgumentException("Token cleanup batch size interval must be at least 1");

        _persistedGrantDbContext = persistedGrantDbContext ?? throw new ArgumentNullException(nameof(persistedGrantDbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _operationalStoreNotification = operationalStoreNotification;
    }

    /// <summary>
    /// Method to clear expired persisted grants.
    /// </summary>
    /// <returns></returns>
    public async Task RemoveExpiredGrantsAsync() {
        try {
            _logger.LogTrace("Querying for expired grants to remove");

            await RemoveGrantsAsync();
            await RemoveDeviceCodesAsync();
        } catch (Exception ex) {
            _logger.LogError("Exception removing expired grants: {exception}", ex.Message);
        }
    }

    /// <summary>
    /// Removes the stale persisted grants.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task RemoveGrantsAsync() {
        var found = int.MaxValue;

        while (found >= _options.TokenCleanupBatchSize) {
            found = await _persistedGrantDbContext.PersistedGrants
                .Where(x => x.Expiration < DateTime.UtcNow)
                .Take(_options.TokenCleanupBatchSize)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Removing {grantCount} grants", found);
        }
    }


    /// <summary>
    /// Removes the stale device codes.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task RemoveDeviceCodesAsync() {
        var found = int.MaxValue;

        while (found >= _options.TokenCleanupBatchSize) {
            found = await _persistedGrantDbContext.DeviceFlowCodes
                .Where(x => x.Expiration < DateTime.UtcNow)
                .OrderBy(x => x.Expiration)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);
        }
    }
}