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

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// Docker feature for diagnostics
    /// </summary>
    public class DiagnosticsFeature : IDockerFeature
    {

        public const string LogLevelSettings = "LEVEL";

        /// <summary>
        /// Gets the identifier of this feature
        /// </summary>
        public string Id => "LOG";

        /// <summary>
        /// Gets the settings for this feature
        /// </summary>
        public IEnumerable<string> Settings => new String[] { LogLevelSettings };

        /// <summary>
        /// Configure the feature
        /// </summary>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {

            var dxConfig = configuration.GetSection<DiagnosticsConfigurationSection>();
            if (dxConfig == null)
            {
                dxConfig = DockerFeatureUtils.LoadConfigurationResource<DiagnosticsConfigurationSection>("SanteDB.Docker.Core.Features.Config.DiagnosticsFeature.xml");
                configuration.AddSection(dxConfig);
            }

            // Set the log level
            if (settings.TryGetValue(LogLevelSettings, out String level))
            {
                if (!Enum.TryParse<System.Diagnostics.Tracing.EventLevel>(level, out System.Diagnostics.Tracing.EventLevel filter))
                {
                    throw new ArgumentOutOfRangeException($"Expected values for {this.Id}_{LogLevelSettings} are Informational, Warning, Error, LogAlways");
                }
                dxConfig.TraceWriter.ForEach(o => o.Filter = filter);
            }

        }
    }
}
