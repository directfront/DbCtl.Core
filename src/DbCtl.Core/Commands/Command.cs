﻿// Copyright 2020 Direct Front Systems (Pty) Ltd.
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

using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Commands
{
    public abstract class Command
    {
        protected DatabaseConnection Connection;

        public Command(DatabaseConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Log.Debug("Creating {command} with {connector} and {connectionstring}", 
                GetType().Name, connection.Connector.GetType().Name, connection.ConnectionString);
        }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
