/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Privacy;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// Privacy filtering 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PrivacyFilterFeature : IDockerFeature
    {

        // Extract property regex
        private Regex m_extractProperty = new Regex(@"^(\w+?)\.(?:(.*?)=)?(.*)?$");

        /// <summary>
        /// Resources to be protected
        /// </summary>
        public const string ResourceTypeSetting = "RESOURCE";

        /// <summary>
        /// Action to take
        /// </summary>
        public const string ActionSetting = "ACTION";

        /// <summary>
        /// Action to take
        /// </summary>
        public const string ForbiddenPropertiesSetting = "FORBID";

        /// <summary>
        /// Gets the id of this feature
        /// </summary>
        public string Id => "DATA_POLICY";

        /// <summary>
        /// Get the settings
        /// </summary>
        public IEnumerable<string> Settings => new String[] { ResourceTypeSetting, ActionSetting };

        /// <summary>
        /// Configure the privacy filter
        /// </summary>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var privacyConf = configuration.GetSection<DataPolicyFilterConfigurationSection>();
            if (privacyConf == null)
            {
                privacyConf = new DataPolicyFilterConfigurationSection() { DefaultAction = ResourceDataPolicyActionType.Hide };
                configuration.AddSection(privacyConf);
            }

            // Action settings
            if (settings.TryGetValue(ActionSetting, out string action))
            {
                if (!Enum.TryParse(action, true, out ResourceDataPolicyActionType actionEnum))
                {
                    throw new ArgumentOutOfRangeException($"{action} is not recognized as an action (valid: NONE, AUDIT, REDACT, NULLIFY, HIDE, ERROR, HASH)");
                }
                privacyConf.DefaultAction = actionEnum;
            }

            if (settings.TryGetValue(ResourceTypeSetting, out string resources))
            {
                privacyConf.Resources = resources.Split(';').Select(o =>
                {

                    var parts = o.Split('=');
                    return new ResourceDataPolicyFilter()
                    {
                        ResourceType = new ResourceTypeReferenceConfiguration()
                        {
                            TypeXml = parts[0]
                        },
                        Action = parts.Length > 1 ? (ResourceDataPolicyActionType)Enum.Parse(typeof(ResourceDataPolicyActionType), parts[1], true) : privacyConf.DefaultAction
                    };
                }).ToList();
            }

            if (settings.TryGetValue(ForbiddenPropertiesSetting, out string forbiddenProperties))
            {
                forbiddenProperties.Split(';').ToList().ForEach(p =>
                {
                    var extract = this.m_extractProperty.Match(p);
                    if (!extract.Success)
                    {
                        throw new InvalidOperationException("SDB_DATA_POLICY_FORBID muust be in format: Resource.property[=Policy OID]");
                    }
                    else
                    {
                        var resourceConfig = privacyConf.Resources.Find(o => o.ResourceType.TypeXml == extract.Groups[1].Value);
                        if (resourceConfig == null)
                        {
                            resourceConfig = new ResourceDataPolicyFilter()
                            {
                                ResourceType = new ResourceTypeReferenceConfiguration() { TypeXml = extract.Groups[1].Value },
                                Action = ResourceDataPolicyActionType.None
                            };
                        }

                        /*
                        resourceConfig.Properties 
                        */

                    }
                });
            }

            // Add services
            var serviceConfiguration = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            if (!serviceConfiguration.Any(s => s.Type == typeof(DataPolicyFilterService)))
            {
                serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(DataPolicyFilterService)));
            }
        }
    }
}
