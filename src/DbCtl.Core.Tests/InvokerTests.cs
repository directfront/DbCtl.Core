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
using DbCtl.Core.Commands;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests
{
    [TestFixture]
    public class When_calling_execute_async_on_invoker
    {
        [Test]
        public async Task It_shoud_execute_the_command()
        {
            var connectionString = "connection-string";
            var connector = new Mock<IDbConnector>();
            var command = new Mock<Command>(new DatabaseConnection(connector.Object, connectionString));
            var ct = new CancellationToken();

            var invoker = new Invoker();
                        
            await invoker.ExecuteAsync(command.Object, ct);

            command.Verify(cmd => cmd.ExecuteAsync(ct));
        }
    }
}
