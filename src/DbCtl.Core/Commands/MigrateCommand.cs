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
using DbCtl.Core.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Commands
{
    public class MigrateCommand : Command
    {
        private readonly IMigrationScriptService _MigrationScriptService;
        private readonly IDatabaseVersionService _DatabaseVersionService;
        private readonly DatabaseConnection _Connection;

        public MigrationType Type { get; set; } = MigrationType.Forward;

        public MigrateCommand(DatabaseConnection connection, IMigrationScriptService migrationScriptService, IDatabaseVersionService databaseVersionService) 
            : base(connection)
        {
            _Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _MigrationScriptService = migrationScriptService ?? throw new ArgumentNullException(nameof(migrationScriptService));
            _DatabaseVersionService = databaseVersionService ?? throw new ArgumentNullException(nameof(databaseVersionService));
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var currentVersion = await _DatabaseVersionService.GetCurrentVersionAsync(cancellationToken);
            var scripts = _MigrationScriptService.FindScripts(currentVersion);

            if (!scripts.Any())
            {
                Log.Information("No scripts found to execute against the database.");
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            await ExecuteScripts(scripts, cancellationToken);

            Log.Information("Done executing {count} scripts", scripts.Count());
        }

        private async Task ExecuteScripts(IEnumerable<string> scripts, CancellationToken cancellationToken)
        {
            foreach (var script in scripts)
            {
                Log.Information("Executing script {script}", script);

                var (Entry, Contents) = await _MigrationScriptService.GetScriptAsync(script, cancellationToken);
                await _Connection.Connector.ExecuteScriptAsync(_Connection.ConnectionString, Contents, cancellationToken);
                await _Connection.Connector.AddChangeLogEntryAsync(_Connection.ConnectionString, Entry, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
