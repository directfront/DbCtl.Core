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
using SemVersion;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("DbCtl.Core.Tests")]

namespace DbCtl.Core.Services
{
    public interface IMigrationScriptService
    {
        IEnumerable<string> FindScripts(SemanticVersion version);
        Task<(ChangeLogEntry Entry, string Contents)> GetScriptAsync(string filename, CancellationToken cancellationToken);
    }

    internal class MigrationScriptService : IMigrationScriptService
    {
        private readonly IFileSystem _FileSystem;
        private readonly string _ScriptsPath;
        private readonly MigrationType _MigrationType;

        public MigrationScriptService(IFileSystem fileSystem, string scriptsPath, MigrationType type)
        {
            _FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _ScriptsPath = string.IsNullOrEmpty(scriptsPath) ? throw new ArgumentNullException(nameof(scriptsPath)) : scriptsPath;
            _MigrationType = type;
        }

        public IEnumerable<string> FindScripts(SemanticVersion version)
        {
            Log.Debug("Finding {type} migration scripts in {path}", _MigrationType, _ScriptsPath);

            if (!_FileSystem.Directory.Exists(_ScriptsPath))
                throw new Exception($"Failed to find path {_ScriptsPath}.");

            var scripts = FindScriptsGreaterThan(version);
            var orderedScripts = OrderScriptsForMigrationType(scripts);

            Log.Debug("Found {count} scripts to execute (ordered): {scripts}", orderedScripts.Count(), orderedScripts);

            return orderedScripts;
        }

        private IEnumerable<string> OrderScriptsForMigrationType(IEnumerable<(string filename, string Version)> scripts)
        {
            return _MigrationType == MigrationType.Forward
                ? scripts.OrderBy(item => item.Version, new VersionComparer()).Select(item => item.filename)
                : scripts.OrderByDescending(item => item.Version, new VersionComparer()).Select(item => item.filename);
        }

        private IEnumerable<(string filename, string Version)> FindScriptsGreaterThan(SemanticVersion version)
        {
            return _FileSystem.Directory
                .EnumerateFiles(_ScriptsPath, _MigrationType == MigrationType.Forward ? "F-*" : "B-*", SearchOption.AllDirectories)
                .Select(filename => (filename, FilenameParser.Parse(filename).Version))
                .Where(item => item.Version > version);
        }

        public async Task<(ChangeLogEntry Entry, string Contents)> GetScriptAsync(string filename, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException(filename);

            var path = Path.Combine(_ScriptsPath, filename);

            Log.Debug("Reading contents of script {path}", path);

            if (File.Exists(path))
            {
                var contents = await File.ReadAllTextAsync(path, cancellationToken);
                Log.Debug("Read {bytes} bytes from {path}", contents.Length, filename);
                return (new ChangeLogEntry(path), contents);
            }

            throw new FileNotFoundException("Failed to find script file.", filename);
        }
    }
}
