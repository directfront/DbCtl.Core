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
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests.Commands
{
    [TestFixture]
    public class When_calling_execute_async_on_initialize_command
    {
        private const string ConnectionString = "db-connection-string";
        private readonly DateTime _ChangeDateTime = new DateTime(2020, 02, 22);

        private Command _Command;
        private Mock<IDbConnector> _Connector;
        private CancellationToken _CancellationToken;

        [SetUp]
        public async Task Setup()
        {
            _Connector = new Mock<IDbConnector>();
            var connection = new DatabaseConnection(_Connector.Object, ConnectionString);

            var changeDateTimeProvider = new Mock<IChangeDateTimeProvider>();
            changeDateTimeProvider.SetupGet(p => p.Now).Returns(_ChangeDateTime);

            _Command = new InitializeCommand(connection, changeDateTimeProvider.Object);
            
            _CancellationToken = new CancellationToken();

            await _Command.ExecuteAsync(_CancellationToken);
        }

        [Test]
        public void It_should_create_the_changelog_table()
        {
            _Connector.Verify(connector => connector.CreateChangeLogTableAsync(ConnectionString, _CancellationToken));
        }

        [Test]
        public void It_should_add_the_intialize_change_log_entry()
        {
            _Connector.Verify(connector => connector.AddChangeLogEntryAsync(ConnectionString, It.Is<ChangeLogEntry>(entry => Match(entry)), _CancellationToken));
        }

        private bool Match(ChangeLogEntry changeLogEntry)
        {
            if (changeLogEntry == null)
                return false;

            var expected = new ChangeLogEntry("f-1.0.0-Initialize_database_change_log.ddl", "DbCtl", _ChangeDateTime, Stream.Null);

            return expected.Equals(changeLogEntry);
        }
    }
}
