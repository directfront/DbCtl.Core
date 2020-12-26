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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Commands
{
    public class InitializeCommand : Command
    {
        private readonly IChangeDateTimeProvider _ChangeDateTimeProvider;

        public InitializeCommand(DatabaseConnection connection)
            : this(connection, new ChangeDateTimeProvider())
        {
        }

        public InitializeCommand(DatabaseConnection connection, IChangeDateTimeProvider changeDateTimeProvider)
            : base(connection)
        {
            _ChangeDateTimeProvider = changeDateTimeProvider ?? throw new ArgumentNullException(nameof(changeDateTimeProvider));
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Log.Information("Initializing the database {connection}", Connection.ToString());

            var changeLogEntry = new ChangeLogEntry("f-1.0.0-Initialize_database_change_log.ddl", "DbCtl", _ChangeDateTimeProvider.Now, Stream.Null);
            await Connection.Connector.CreateChangeLogTableAsync(Connection.ConnectionString, cancellationToken);
            await Connection.Connector.AddChangeLogEntryAsync(Connection.ConnectionString, changeLogEntry, cancellationToken);
            
            Log.Information("Completed initializing the database");
        }
    }
}
