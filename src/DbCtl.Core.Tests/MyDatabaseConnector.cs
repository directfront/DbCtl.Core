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
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests
{
    [ExportMetadata("Name", "my-db-connector")]
    [ExportMetadata("Description", "Test database connector")]
    [ExportMetadata("Version", "1.0.0")]
    [Export(typeof(IDbConnector))]
    public class MyDatabaseConnector : IDbConnector
    {
        public Task<int> AddChangeLogEntryAsync(string connectionString, ChangeLogEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateChangeLogTableAsync(string connectionString, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteScriptAsync(string connectionString, string script, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChangeLogEntry>> FetchChangeLogEntriesAsync(string connectionString, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
