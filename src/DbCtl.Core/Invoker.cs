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

using DbCtl.Core.Commands;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core
{
    public class Invoker
    {
        public async Task ExecuteAsync(Command command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            Log.Information("Executing {command}", command.GetType().Name);

            await command.ExecuteAsync(cancellationToken);
        }
    }
}
