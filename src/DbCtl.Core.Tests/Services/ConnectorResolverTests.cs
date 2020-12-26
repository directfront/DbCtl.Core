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

using DbCtl.Core.Gateways;
using DbCtl.Core.Services;
using Moq;
using NUnit.Framework;
using System;
using System.ComponentModel.Composition.Hosting;

namespace DbCtl.Core.Tests.Services
{
    public class When_calling_resolve_on_connector_resolver
    {
        private ConnectorResolver _Resolver;

        [SetUp]
        public void Setup()
        {
            var gateway = new Mock<IConnectorsGateway>();
            gateway.Setup(gw => gw.FindConnectors()).Returns(new[] { new AssemblyCatalog(typeof(MyDatabaseConnector).Assembly) });
            _Resolver = new ConnectorResolver(gateway.Object);
        }

        [Test]
        public void It_should_resolve_the_connector_by_name()
        {
            var connector = _Resolver.Resolve("my-db-connector");
            Assert.IsInstanceOf<MyDatabaseConnector>(connector);
        }

        [Test]
        public void It_should_throw_an_exception_when_the_connector_is_not_resolved()
        {
            var exception = Assert.Throws<Exception>(() => _Resolver.Resolve("non-existent-connector"));
            Assert.AreEqual("Failed to find the non-existent-connector connector implementation.", exception.Message);
        }
    }
}