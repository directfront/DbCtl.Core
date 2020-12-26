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

using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.IO.Abstractions;

namespace DbCtl.Core.Gateways
{
    public interface IConnectorsGateway
    {
        IEnumerable<ComposablePartCatalog> FindConnectors();
    }

    public class ConnectorsGateway : IConnectorsGateway
    {
        private const string ConnectorsSubdirectory = "connectors";
        private readonly IFileSystem _FileSystem;
        private readonly Func<string, ComposablePartCatalog> _CatalogFactory;

        public ConnectorsGateway(IFileSystem fileSystem, Func<string, ComposablePartCatalog> catalogFactory)
        {
            _FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _CatalogFactory = catalogFactory ?? throw new ArgumentNullException(nameof(catalogFactory));
        }

        public virtual IEnumerable<ComposablePartCatalog> FindConnectors()
        {
            if (_FileSystem.Directory.Exists(ConnectorsSubdirectory))
            {
                Log.Information("Processing connectors from {directory}", Path.Combine(Directory.GetCurrentDirectory(), ConnectorsSubdirectory));
                foreach (var directory in _FileSystem.Directory.EnumerateDirectories(ConnectorsSubdirectory, "*", SearchOption.AllDirectories))
                    yield return _CatalogFactory(directory);
            }
            else
                Log.Warning("Skipped processing {connectorsDirectory} directory as it does not exist.", ConnectorsSubdirectory);
        }
    }
}
