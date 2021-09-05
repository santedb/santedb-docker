/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
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
 * User: fyfej
 * Date: 2021-8-5
 */
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Docker.Core
{
    /// <summary>
    /// Represents a grouping of features which can be configured via Docker environment variable
    /// </summary>
    /// <remarks>
    /// This interface allows a plugin to expose one or more services via the SDB_ENABLE=x,y,z environment variable.
    /// </remarks>
    public interface IDockerFeature
    {

        /// <summary>
        /// Gets the identifier of the feature
        /// </summary>
        /// <remarks>This is the identifier that appears in the SDB_FEATURE</remarks>
        string Id { get; }

        /// <summary>
        /// Get a list of settings allowed for this object
        /// </summary>
        IEnumerable<String> Settings { get; }

        /// <summary>
        /// Configure the feature for execution in Docker
        /// </summary>
        /// <remarks>Allows the feature to be configured against the provided <paramref name="configuration"/></remarks>
        /// <param name="configuration">The configuration into which the feature should be configured</param>
        /// <param name="settings">Settings which were parsed from the environment in SDB_{this.Id}_{Key}={Value}</param>
        void Configure(SanteDBConfiguration configuration, IDictionary<String, String> settings);

    }
}
