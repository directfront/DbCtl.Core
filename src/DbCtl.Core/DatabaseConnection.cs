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

namespace DbCtl.Core
{
    public class DatabaseConnection
    {
        public DatabaseConnection(IDbConnector connector, string connectionString)
        {
            Connector = connector ?? throw new ArgumentNullException(nameof(connector));
            ConnectionString = string.IsNullOrEmpty(connectionString) ? throw new ArgumentNullException(nameof(connectionString)) : connectionString;
        }

        public IDbConnector Connector { get; private set; }
        public string ConnectionString { get; private set; }

        public override string ToString()
        {
            return $"Connection => {Connector.GetType().Name} -> {ConnectionString}";
        }
    }
}
