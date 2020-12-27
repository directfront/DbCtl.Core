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
using DbCtl.Core.Services;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests.Commands
{
    [TestFixture]
    public class When_calling_execute_async_on_migrate_command
    {
        private const string ConnectionString = "datbase-connection-string";
        private Mock<IDbConnector> _Connector;
        private Mock<IMigrationScriptService> _MigrationScriptService;
        private Mock<IDatabaseVersionService> _DatabaseVersionService;
        private MigrateCommand _Command;

        [SetUp]
        public void Setup()
        {
            _Connector = new Mock<IDbConnector>();
            var connection = new DatabaseConnection(_Connector.Object, ConnectionString);

            _MigrationScriptService = new Mock<IMigrationScriptService>();
            _DatabaseVersionService = new Mock<IDatabaseVersionService>();

            _Command = new MigrateCommand(connection, _MigrationScriptService.Object, _DatabaseVersionService.Object);
        }

        [Test]
        public async Task It_should_execute_the_missing_forward_migration_scripts_against_the_database()
        {
            var changeLogEntry1 = new ChangeLogEntry("f-1.0.1-script.ddl", "DbCtl", new System.DateTime(2020, 02, 14), Stream.Null);
            var changeLogEntry2 = new ChangeLogEntry("f-1.1.2-script.ddl", "DbCtl", new System.DateTime(2020, 02, 14), Stream.Null);

            var cancellationToken = new CancellationToken();

            _DatabaseVersionService.Setup(dbvs => dbvs.GetCurrentVersionAsync(cancellationToken)).ReturnsAsync("1.0.0");
            _MigrationScriptService.Setup(mss => mss.FindScripts("1.0.0")).Returns(new[] { 
                "f-1.0.1-script.ddl", 
                "f-1.1.2-script.ddl"
            });

            _MigrationScriptService.Setup(mss => mss.GetScriptAsync("f-1.0.1-script.ddl", cancellationToken))
                .ReturnsAsync((changeLogEntry1, "script-contents-1"));

            _MigrationScriptService.Setup(mss => mss.GetScriptAsync("f-1.1.2-script.ddl", cancellationToken))
                .ReturnsAsync((changeLogEntry2, "script-contents-2"));

            _Command.Type = MigrationType.Forward;
            await _Command.ExecuteAsync(cancellationToken);

            _Connector.Verify(connector => connector.ExecuteScriptAsync(ConnectionString, "script-contents-1", cancellationToken));
            _Connector.Verify(connector => connector.AddChangeLogEntryAsync(ConnectionString, changeLogEntry1, cancellationToken));

            _Connector.Verify(connector => connector.ExecuteScriptAsync(ConnectionString, "script-contents-2", cancellationToken));
            _Connector.Verify(connector => connector.AddChangeLogEntryAsync(ConnectionString, changeLogEntry2, cancellationToken));
        }

        [Test]
        public async Task It_should_execute_the_current_backward_migration_script_against_the_database()
        {
            var changeLogEntry = new ChangeLogEntry("b-1.1.2-script.ddl", "DbCtl", new System.DateTime(2020, 02, 16), Stream.Null);

            var cancellationToken = new CancellationToken();

            _DatabaseVersionService.Setup(dbvs => dbvs.GetCurrentVersionAsync(cancellationToken)).ReturnsAsync("1.1.2");
            _MigrationScriptService.Setup(mss => mss.FindScripts("1.1.2")).Returns(new[] {
                "b-1.1.2-script.ddl"
            });

            _MigrationScriptService.Setup(mss => mss.GetScriptAsync("b-1.1.2-script.ddl", cancellationToken))
                .ReturnsAsync((changeLogEntry, "script-contents"));

            _Command.Type = MigrationType.Backward;
            await _Command.ExecuteAsync(cancellationToken);

            _Connector.Verify(connector => connector.ExecuteScriptAsync(ConnectionString, "script-contents", cancellationToken));
            _Connector.Verify(connector => connector.AddChangeLogEntryAsync(ConnectionString, changeLogEntry, cancellationToken));
        }

        [Test]
        public async Task It_should_not_execute_any_scripts_if_no_scripts_are_found()
        {
            var cancellationToken = new CancellationToken();

            _DatabaseVersionService.Setup(dbvs => dbvs.GetCurrentVersionAsync(cancellationToken)).ReturnsAsync("1.0.0");
            _MigrationScriptService.Setup(mss => mss.FindScripts("1.0.0")).Returns(Enumerable.Empty<string>());

            await _Command.ExecuteAsync(cancellationToken);

            _Connector.VerifyNoOtherCalls();
        }
    }
}
