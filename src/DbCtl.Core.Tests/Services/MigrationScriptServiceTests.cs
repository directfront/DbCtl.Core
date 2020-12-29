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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbCtl.Core.Tests.Services
{
    [TestFixture]
    public class When_calling_find_scripts_on_migration_script_service
    {
        private Mock<IChangeDateTimeProvider> _ChangeDateTimeProvider;
        private Mock<IFileSystem> _FileSystem;
        private Mock<IDirectory> _Directory;

        [SetUp]
        public void Setup()
        {
            _ChangeDateTimeProvider = new Mock<IChangeDateTimeProvider>();
            _Directory = new Mock<IDirectory>();
            _FileSystem = new Mock<IFileSystem>();
            _FileSystem.Setup(fs => fs.Directory).Returns(_Directory.Object);        
        }

        [Test]
        public void It_should_throw_an_exception_when_the_scripts_path_does_not_exist()
        {
            _Directory.Setup(d => d.Exists("scripts")).Returns(false);

            var service = new MigrationScriptService(_FileSystem.Object, "scripts", MigrationType.Forward, _ChangeDateTimeProvider.Object);

            var exception = Assert.Throws<Exception>(() => service.FindScripts("1.2.3"));
            Assert.AreEqual("Failed to find path scripts.", exception.Message);
        }

        [Test]
        public void It_should_strip_the_path_from_the_filename()
        {
            var scriptsDirectory = "scripts";

            _Directory.Setup(d => d.Exists(scriptsDirectory)).Returns(true);

            _Directory.Setup(d => d.EnumerateFiles(scriptsDirectory, "F-*", SearchOption.AllDirectories)).Returns(new[]
            {
                Path.Combine(scriptsDirectory, "f-1.0.1-one.ddl"),
                Path.Combine(scriptsDirectory, "f-1.0.2-two.ddl")
            });

            var service = new MigrationScriptService(_FileSystem.Object, scriptsDirectory, MigrationType.Forward, _ChangeDateTimeProvider.Object);
            var scriptsToRun = service.FindScripts("1.0.1");

            var expected = new[] {
                "f-1.0.2-two.ddl"
            };

            CollectionAssert.AreEqual(expected, scriptsToRun);
        }

        [Test]
        public void It_should_find_the_forward_scripts_after_the_specified_version()
        {
            _Directory.Setup(d => d.Exists("scripts")).Returns(true);

            _Directory.Setup(d => d.EnumerateFiles("scripts", "F-*", SearchOption.AllDirectories)).Returns(new[]
            {
                "f-1.0.1-one.ddl",
                "f-1.1.0-three.ddl",
                "f-1.0.2-two.ddl",
                "f-1.1.2-five.ddl",
                "f-1.1.1-four.ddl",
            });

            var service = new MigrationScriptService(_FileSystem.Object, "scripts", MigrationType.Forward, _ChangeDateTimeProvider.Object);
            var scriptsToRun = service.FindScripts("1.0.2");

            var expected = new[] {
                "f-1.1.0-three.ddl",
                "f-1.1.1-four.ddl",
                "f-1.1.2-five.ddl",
            };

            CollectionAssert.AreEqual(expected, scriptsToRun);
        }

        [Test]
        public void It_should_find_the_current_backward_script()
        {
            _Directory.Setup(d => d.Exists("scripts")).Returns(true);

            _Directory.Setup(d => d.EnumerateFiles("scripts", "B-*", SearchOption.AllDirectories)).Returns(new[]
            {
                "b-1.0.1-one.ddl",
                "b-1.1.0-three.ddl",
                "b-1.0.2-two.ddl",
                "b-1.1.2-five.ddl",
                "b-1.1.1-four.ddl",
            });

            var service = new MigrationScriptService(_FileSystem.Object, "scripts", MigrationType.Backward, _ChangeDateTimeProvider.Object);
            var scriptsToRun = service.FindScripts("1.1.0");

            var expected = new[] {
                "b-1.1.0-three.ddl"
            };

            CollectionAssert.AreEqual(expected, scriptsToRun);
        }
    }

    [TestFixture]
    public class When_calling_get_scripts_async_on_migration_script_service
    {
        private DateTime _ChangeDateTime = new DateTime(2020, 02, 12);
        private Mock<IChangeDateTimeProvider> _ChangeDateTimeProvider;
        private CancellationToken _CancellationToken = new CancellationToken();
        private Mock<IFileSystem> _FileSystem;
        private Mock<IFile> _File;

        [SetUp]
        public void Setup()
        {
            _ChangeDateTimeProvider = new Mock<IChangeDateTimeProvider>();
            _ChangeDateTimeProvider.Setup(p => p.Now).Returns(_ChangeDateTime);

            _File = new Mock<IFile>();
            _FileSystem = new Mock<IFileSystem>();
            _FileSystem.SetupGet(fs => fs.File).Returns(_File.Object);
        }

        [Test]
        public void It_should_throw_an_exception_when_the_script_does_not_exist()
        {
            _File.Setup(f => f.Exists(It.IsAny<string>())).Returns(false);

            var service = new MigrationScriptService(_FileSystem.Object, "scripts", MigrationType.Forward, _ChangeDateTimeProvider.Object);

            var exception = Assert.ThrowsAsync<FileNotFoundException>(async () => await service.GetScriptAsync(@".\scripts\f-1.0.1-one.ddl", _CancellationToken));
            Assert.AreEqual("Failed to find script file.", exception.Message);
        }

        [Test]
        public async Task It_should_return_the_contents_of_the_script_file_and_a_corresponding_change_log_entry()
        {
            const string scriptFile = @"scripts\f-1.0.1-one.ddl";
            var contents = Encoding.UTF8.GetBytes("SELECT 1");

            _File.Setup(f => f.Exists(scriptFile)).Returns(true);
            _File.Setup(f => f.ReadAllBytesAsync(scriptFile, _CancellationToken)).ReturnsAsync(contents);

            var service = new MigrationScriptService(_FileSystem.Object, "scripts", MigrationType.Forward, _ChangeDateTimeProvider.Object);
            var script = await service.GetScriptAsync("f-1.0.1-one.ddl", _CancellationToken);

            Assert.AreEqual(contents, script.Contents);
            Assert.AreEqual("1.0.1", script.Entry.Version);
            Assert.AreEqual("one", script.Entry.Description);
            Assert.AreEqual(_ChangeDateTime, script.Entry.ChangeDateTime);
            Assert.AreEqual("f-1.0.1-one.ddl", script.Entry.Filename);
            Assert.AreEqual(MigrationType.Forward, script.Entry.MigrationType);
            Assert.AreEqual("b1698e52a0f16203489454196a0c6307", script.Entry.Hash);
            Assert.AreEqual(Environment.UserName, script.Entry.AppliedBy);
        }
    }
}