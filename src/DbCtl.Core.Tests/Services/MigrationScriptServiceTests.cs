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
using System.IO.Abstractions;

namespace DbCtl.Core.Tests.Services
{
    [TestFixture]
    public class When_calling_find_scripts_on_migration_script_service
    {
        [Test]
        public void It_should_throw_an_exception_when_the_scripts_path_does_not_exist()
        {
            var fileSystem = new Mock<IFileSystem>();
            var service = new MigrationScriptService(fileSystem.Object, "scripts", MigrationType.Forward);

            var directory = new Mock<IDirectory>();
            directory.Setup(d => d.Exists("scripts")).Returns(false);
            fileSystem.Setup(fs => fs.Directory).Returns(directory.Object);

            var exception = Assert.Throws<Exception>(() => service.FindScripts("1.2.3"));
            Assert.AreEqual("Failed to find path scripts.", exception.Message);
        }

        [Test]
        public void It_should_strip_the_path_from_the_filename()
        {
            var fileSystem = new Mock<IFileSystem>();
            var service = new MigrationScriptService(fileSystem.Object, "scripts", MigrationType.Forward);

            var directory = new Mock<IDirectory>();
            directory.Setup(d => d.Exists("scripts")).Returns(true);
            fileSystem.Setup(d => d.Directory).Returns(directory.Object);

            directory.Setup(d => d.EnumerateFiles("scripts", "F-*", SearchOption.AllDirectories)).Returns(new[]
            {
                @".\scripts\f-1.0.1-one.ddl",
                @".\scripts\f-1.0.2-two.ddl",
            });

            var scriptsToRun = service.FindScripts("1.0.1");

            var expected = new[] {
                "f-1.0.2-two.ddl"
            };

            CollectionAssert.AreEqual(expected, scriptsToRun);
        }

        [Test]
        public void It_should_find_the_forward_scripts_after_the_specified_version()
        {
            var fileSystem = new Mock<IFileSystem>();
            var service = new MigrationScriptService(fileSystem.Object, "scripts", MigrationType.Forward);

            var directory = new Mock<IDirectory>();
            directory.Setup(d => d.Exists("scripts")).Returns(true);
            fileSystem.Setup(d => d.Directory).Returns(directory.Object);

            directory.Setup(d => d.EnumerateFiles("scripts", "F-*", SearchOption.AllDirectories)).Returns(new[]
            {
                "f-1.0.1-one.ddl",
                "f-1.1.0-three.ddl",
                "f-1.0.2-two.ddl",
                "f-1.1.2-five.ddl",
                "f-1.1.1-four.ddl",
            });

            var scriptsToRun = service.FindScripts("1.0.2");

            var expected = new[] {
                "f-1.1.0-three.ddl",
                "f-1.1.1-four.ddl",
                "f-1.1.2-five.ddl",
            };

            CollectionAssert.AreEqual(expected, scriptsToRun);
        }

        [Test]
        public void It_should_find_the_backward_scripts_up_until_the_specified_version()
        {
            var fileSystem = new Mock<IFileSystem>();
            var service = new MigrationScriptService(fileSystem.Object, "scripts", MigrationType.Backward);

            var directory = new Mock<IDirectory>();
            directory.Setup(d => d.Exists("scripts")).Returns(true);
            fileSystem.Setup(d => d.Directory).Returns(directory.Object);

            directory.Setup(d => d.EnumerateFiles("scripts", "B-*", SearchOption.AllDirectories)).Returns(new[]
            {
                "b-1.0.1-one.ddl",
                "b-1.1.0-three.ddl",
                "b-1.0.2-two.ddl",
                "b-1.1.2-five.ddl",
                "b-1.1.1-four.ddl",
            });

            var scriptsToRun = service.FindScripts("1.1.0");

            var expected = new[] {
                "b-1.1.2-five.ddl",
                "b-1.1.1-four.ddl"
            };

            CollectionAssert.AreEqual(expected, scriptsToRun);
        }
    }
}
