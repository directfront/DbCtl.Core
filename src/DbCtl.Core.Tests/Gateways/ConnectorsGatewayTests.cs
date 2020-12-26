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
using Moq;
using NUnit.Framework;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace DbCtl.Core.Tests.Gateways
{
    [TestFixture]
    public class When_calling_find_connectors_on_connectors_gateway
    {
        private Mock<IFileSystem> _FileSystem;
        private Mock<IDirectory> _Directory;

        [SetUp]
        public void Setup()
        {
            _Directory = new Mock<IDirectory>();
            _FileSystem = new Mock<IFileSystem>();
            _FileSystem.Setup(fs => fs.Directory).Returns(_Directory.Object);
        }

        [Test]
        public void It_should_skip_the_connectors_directory_if_it_does_not_exist()
        {
            var gateway = new ConnectorsGateway(_FileSystem.Object, d => new DirectoryCatalog(d));
            _Directory.Setup(d => d.Exists("connectors")).Returns(false).Verifiable();
            
            gateway.FindConnectors();
            _Directory.VerifyNoOtherCalls();
        }

        [Test]
        public void It_should_return_an_empty_enumerable_when_the_connectors_directory_does_not_exist()
        {
            var gateway = new ConnectorsGateway(_FileSystem.Object, d => new DirectoryCatalog(d));
            _Directory.Setup(d => d.Exists(It.IsAny<string>())).Returns(false);

            Assert.IsEmpty(gateway.FindConnectors());
        }

        [Test]
        public void It_should_return_enumerable_of_directory_catalogs_in_connectors_directory()
        {
            var gateway = new ConnectorsGateway(_FileSystem.Object, d => new Mock<ComposablePartCatalog>().Object);

            _Directory.Setup(d => d.Exists("connectors")).Returns(true);
            _Directory.Setup(d => d.EnumerateDirectories("connectors", "*", SearchOption.AllDirectories)).Returns(new[] { @"connectors\con-1", @"connectors\con-2" });

            var catalogs = gateway.FindConnectors();

            Assert.AreEqual(2, catalogs.Count());
        }
    }
}
