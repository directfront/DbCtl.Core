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
using Moq;
using NUnit.Framework;

namespace DbCtl.Core.Tests
{
    [TestFixture]
    public class When_creating_a_database_connection
    {
        private const string ConnectionString = "db-connection-string";
        private IDbConnector _Connector;
        private DatabaseConnection _Connection;

        [SetUp]
        public void Setup()
        {
            _Connector = new Mock<IDbConnector>().Object;
            _Connection = new DatabaseConnection(_Connector, ConnectionString);
        }

        [Test]
        public void It_should_set_the_connector()
        {
            Assert.AreEqual(_Connector, _Connection.Connector);
        }

        [Test]
        public void It_should_set_the_connection_string()
        {
            Assert.AreEqual(ConnectionString, _Connection.ConnectionString);
        }
    }
}
