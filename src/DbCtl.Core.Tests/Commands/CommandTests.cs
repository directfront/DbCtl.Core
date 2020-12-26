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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests.Commands
{
    [TestFixture]
    public class When_constructing_a_command
    {
        private class TestCommand : Command
        {
            public TestCommand(IDbConnector connector, string connectionString)
                : base(new DatabaseConnection(connector, connectionString))
            {
            }

            public DatabaseConnection GetConnection() => Connection;

            public override Task ExecuteAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        private const string ConnectionString = "db-connection-string";
        private TestCommand _Command;
        private Mock<IDbConnector> _Connector;

        [SetUp]
        public void Setup()
        {
            _Connector = new Mock<IDbConnector>();
            _Command = new TestCommand(_Connector.Object, ConnectionString);
        }

        [Test]
        public void It_should_set_the_connector()
        {
            Assert.AreEqual(_Connector.Object, _Command.GetConnection().Connector);
        }

        [Test]
        public void It_should_set_the_connection_string()
        {
            Assert.AreEqual(ConnectionString, _Command.GetConnection().ConnectionString);
        }
    }
}
