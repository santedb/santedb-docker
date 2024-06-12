/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.BI.Services.Impl;
using SanteDB.BusinessRules.JavaScript;
using SanteDB.Core.Applets.Services.Impl;
using SanteDB.Core.Configuration;
using SanteDB.Core.Data.Initialization;
using SanteDB.Core.Notifications;
using SanteDB.Core.PubSub.Broker;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Privacy;
using SanteDB.Core.Security.Tfa;
using SanteDB.Core.Services.Impl;
using SanteDB.Core.Services.Impl.Repository;
using SanteDB.Docker.Core;
using SanteDB.Rest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using SanteDB.Core.Diagnostics.Tracing;
using SanteDB.Cdss.Xml;

namespace SanteDB.Docker.Server.Features
{
    /// <summary>
    /// Core docker feature
    /// </summary>
    public class CoreDockerFeature : IDockerFeature
    {
        private readonly Type[] m_serviceTypes =
        {
            typeof(SanteDB.Core.Security.DefaultPolicyEnforcementService),
            typeof(DefaultOperatingSystemInfoService),
            typeof(DataInitializationService),
            typeof(DefaultThreadPoolService),
            typeof(DefaultNetworkInformationService),
            typeof(RestServiceFactory),
            typeof(RolloverLogManagerService),
            typeof(LocalRepositoryFactory),
            typeof(ExemptablePolicyFilterService),
            typeof(LocalMailMessageService),
            typeof(LocalStockManagementRepositoryService),
            typeof(PubSubBroker),
            typeof(AppletBiRepository),
            typeof(LocalBiRenderService),
            typeof(DefaultDatamartManager),
            typeof(InMemoryPivotProvider),
            typeof(DefaultDatamartManager),
            typeof(DefaultNotificationService),
            typeof(LocalTemplateDefinitionRepositoryService),
            typeof(DefaultDataSigningService),
            typeof(AesSymmetricCrypographicProvider),
            typeof(Rfc4226TfaCodeProvider),
            typeof(DefaultTfaService),
            typeof(LocalRepositoryFactory),
            typeof(AppletLocalizationService),
            typeof(CachedResourceCheckoutService),
            typeof(AppletForeignDataMapRepository),
            typeof(AppletBusinessRulesDaemon),
            typeof(FileSystemAppletManagerService),
            typeof(AppletNotificationTemplateRepository),
            typeof(AppletSubscriptionRepository),
            typeof(AuditDaemonService),
            typeof(AppletDatasetProvider),
            typeof(AppletTemplateDefinitionInstaller),
            typeof(AppletClinicalProtocolInstaller),
            typeof(FileSystemDatasetProvider),
            typeof(DataInitializationService),
            typeof(MonoPlatformSecurityProvider)
        };

        /// <inheritdoc/>
        public string Id => "CORE";

        /// <inheritdoc/>
        public IEnumerable<string> Settings { get { yield break; } }

        /// <inheritdoc/>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            if (appService == null)
            {
                appService = new ApplicationServiceContextConfigurationSection()
                {
                    ThreadPoolSize = Environment.ProcessorCount,
                    InstanceName = "docker",
                    AppSettings = new List<AppSettingKeyValuePair>(),
                    ServiceProviders = new List<TypeReferenceConfiguration>()
                };
                configuration.AddSection(appService);
            }
            foreach (var itm in this.m_serviceTypes)
            {
                if (!appService.ServiceProviders.Any(t => t.Type == itm))
                {
                    appService.ServiceProviders.Add(new TypeReferenceConfiguration(itm));
                }
            }
        }
    }
}
