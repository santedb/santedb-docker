/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Portions Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you
 * may not use this file except in compliance with the License. You may
 * obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 *
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.PubSub.Broker;
using SanteDB.Docker.Core;
using SanteDB.Persistence.PubSub.ADO.Configuration;
using SanteDB.Persistence.PubSub.ADO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Services.Impl;

namespace SanteDB.Docker.Server.Features
{
    /// <summary>
    /// A docker feature which registers the file system queue 
    /// </summary>
    public class FileSystemQueueDockerFeature : IDockerFeature
    {
        /// <inheritdoc/>
        public string Id => "FS_MQ";

        /// <inheritdoc/>
        public IEnumerable<string> Settings => new String[0];

        /// <inheritdoc/>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {

            // Add service for persisting
            var serviceConfiguration = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            if (!serviceConfiguration.Any(s => s.Type == typeof(FileSystemDispatcherQueueService)))
            {
                serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(FileSystemDispatcherQueueService)));
            }
        }
    }
}
