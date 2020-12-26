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
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests.Services
{
    [TestFixture]
    public class When_calling_get_current_version_async_on_database_version_serviceTests
    {
        private const string ConnectionString = "db-connection-string";
        private Mock<IDbConnector> _Connector;
        private CancellationToken _CancellationToken;
        private DatabaseConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _CancellationToken = new CancellationToken();
            _Connector = new Mock<IDbConnector>();
            _Connection = new DatabaseConnection(_Connector.Object, ConnectionString);
        }

        [Test]
        public async Task It_should_return_the_latest_database_version()
        {
            _Connector.Setup(connector => connector.FetchChangeLogEntriesAsync(ConnectionString, _CancellationToken)).ReturnsAsync(new[]
            {
                new ChangeLogEntry("f-1.2.1-two.ddl", "DbCtl", DateTime.Now, Stream.Null),
                new ChangeLogEntry("f-1.2.0-one.ddl", "DbCtl", DateTime.Now, Stream.Null),
            });

            var service = new DatabaseVersionService(_Connection);

            var version = await service.GetCurrentVersionAsync(_CancellationToken);

            Assert.AreEqual("1.2.1", version);
        }

        [Test]
        public async Task It_should_return_the_latest_database_version_taking_into_account_backward_migrations()
        {
            _Connector.Setup(connector => connector.FetchChangeLogEntriesAsync(ConnectionString, _CancellationToken)).ReturnsAsync(new[]
            {
                new ChangeLogEntry("f-1.2.1-two.ddl", "DbCtl", DateTime.Now, Stream.Null),
                new ChangeLogEntry("b-1.2.3-undo_one.dml", "DbCtl", DateTime.Now, Stream.Null),
                new ChangeLogEntry("f-1.2.0-one.ddl", "DbCtl", DateTime.Now, Stream.Null),
                new ChangeLogEntry("f-1.2.3-one.dml", "DbCtl", DateTime.Now, Stream.Null),
                new ChangeLogEntry("f-1.2.2-one.dcl", "DbCtl", DateTime.Now, Stream.Null)
            });

            var service = new DatabaseVersionService(_Connection);

            var version = await service.GetCurrentVersionAsync(_CancellationToken);

            Assert.AreEqual("1.2.2", version);
        }
    }
}
