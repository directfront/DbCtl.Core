// Copyright 2020 Direct Front Systems (Pty) Ltd.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DbCtl.Connectors;
using SemVersion;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Services
{
    public interface IDatabaseVersionService
    {
        Task<string> GetCurrentVersionAsync(CancellationToken cancellationToken);
    }

    public class DatabaseVersionService
    {
        private readonly DatabaseConnection _Connection;

        public DatabaseVersionService(DatabaseConnection connection)
        {
            _Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<string> GetCurrentVersionAsync(CancellationToken cancellationToken)
        {
            return await FindCurrentDatabaseVersionAsync(cancellationToken);
        }

        private async Task<string> FindCurrentDatabaseVersionAsync(CancellationToken cancellationToken)
        {
            Log.Debug("Calculating the current version of the database {connection}", _Connection.ToString());
            var changes = await FetchAppliedDatabaseChangesAsync(cancellationToken);
            var orderedChanges = OrderChangeLogEntries(changes);
            var lastAppliedVersion = orderedChanges.Last().Version;

            Log.Debug("Last applied script version is {version}", lastAppliedVersion);

            return lastAppliedVersion;
        }

        private async Task<IEnumerable<ChangeLogEntry>> FetchAppliedDatabaseChangesAsync(CancellationToken cancellationToken)
        {
            var changes = await _Connection.Connector.FetchChangeLogEntriesAsync(_Connection.ConnectionString, cancellationToken) ?? Enumerable.Empty<ChangeLogEntry>();
            Log.Debug("Found {count} changes applied to the database", changes.Count());
            ThrowIfNoChangesFound(changes);
            return changes;
        }    

        private static IEnumerable<ChangeLogEntry> OrderChangeLogEntries(IEnumerable<ChangeLogEntry> changes)
        {
            var versionComparer = new VersionComparer();
            var forwardMigrations = FindForwardMigrations(changes, versionComparer);
            var backwardMigrations = FindBackwardMigrations(changes, versionComparer);

            Log.Debug("Found {fcount} forward migrations and {bcount} backward migrations applied to the database",
                forwardMigrations.Count(), backwardMigrations.Count());

            var applied = FindEffectiveForwardMigrations(forwardMigrations, backwardMigrations);

            return applied;
        }

        private static List<ChangeLogEntry> FindEffectiveForwardMigrations(IOrderedEnumerable<ChangeLogEntry> forwardMigrations, IOrderedEnumerable<ChangeLogEntry> backwardMigrations)
        {
            var applied = new List<ChangeLogEntry>();

            foreach (var migration in forwardMigrations)
            {
                if (backwardMigrations.Select(bm => bm.Version).Contains(migration.Version))
                {
                    Log.Debug("Forward migration {fm} was undone", migration.Version);
                    continue;
                }
                else
                {
                    Log.Debug("Forward migration {fm} is still effective", migration.Version);
                    applied.Add(migration);
                }
            }

            return applied;
        }

        private static IOrderedEnumerable<ChangeLogEntry> FindBackwardMigrations(IEnumerable<ChangeLogEntry> changes, VersionComparer versionComparer)
        {
            return changes.Where(e => e.MigrationType == MigrationType.Backward).OrderBy(e => e.Version, versionComparer);
        }

        private static IOrderedEnumerable<ChangeLogEntry> FindForwardMigrations(IEnumerable<ChangeLogEntry> changes, VersionComparer versionComparer)
        {
            return changes.Where(e => e.MigrationType == MigrationType.Forward).OrderBy(e => e.Version, versionComparer);
        }

        private static void ThrowIfNoChangesFound(IEnumerable<ChangeLogEntry> changes)
        {
            if (!changes.Any())
                throw new Exception("Failed to find any change log entries in the database. Was the database initialized with DbCtl?");
        }
    }
}
