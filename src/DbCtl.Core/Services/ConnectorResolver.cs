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
using DbCtl.Core.Gateways;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace DbCtl.Core.Services
{
    public interface IConnectorResolver
    {
        IDbConnector Resolve(string connectorName);
    }

    public class ConnectorResolver : IConnectorResolver
    {
        [ImportMany(typeof(IDbConnector))]
        private IEnumerable<Lazy<IDbConnector, IDbConnectorMetadata>> _DbConnectors = 
            Enumerable.Empty<Lazy<IDbConnector, IDbConnectorMetadata>>();

        private readonly IConnectorsGateway _ConnectorsGateway;

        public ConnectorResolver(IConnectorsGateway connectorsGateway)
        {
            _ConnectorsGateway = connectorsGateway ?? throw new ArgumentNullException(nameof(connectorsGateway));

            var catalog = BuildConnectorsCatalog();
            var container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection);
            container.ComposeParts(this);
        }

        public IDbConnector Resolve(string connectorName)
        {
            if (string.IsNullOrEmpty(connectorName))
                throw new ArgumentNullException(nameof(connectorName));

            foreach (var candidate in _DbConnectors)
            {
                Log.Debug("Discovered connector {name} {version}", candidate.Metadata.Name, candidate.Metadata.Version);

                if (candidate.Metadata.Name.Equals(connectorName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Information("Selected connector {name} {version} {description}", candidate.Metadata.Name, candidate.Metadata.Version, candidate.Metadata.Description);
                    return candidate.Value;
                }
            }

            throw new Exception($"Failed to find the {connectorName} connector implementation.");
        }

        private AggregateCatalog BuildConnectorsCatalog()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new ApplicationCatalog());
            
            foreach (var connectorCatalog in _ConnectorsGateway.FindConnectors())
                catalog.Catalogs.Add(connectorCatalog);

            return catalog;
        }
    }
}
