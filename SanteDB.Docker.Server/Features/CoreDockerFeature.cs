using SanteDB.BI.Services.Impl;
using SanteDB.Core.Applets.Services.Impl;
using SanteDB.Core.Configuration;
using SanteDB.Core.Data.Initialization;
using SanteDB.Core.Notifications;
using SanteDB.Core.PubSub.Broker;
using SanteDB.Core.Security.Privacy;
using SanteDB.Core.Security.Tfa;
using SanteDB.Core.Security;
using SanteDB.Core.Services.Impl.Repository;
using SanteDB.Core.Services.Impl;
using SanteDB.Docker.Core;
using SanteDB.Rest.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.BusinessRules.JavaScript;
using SanteDB.Core.Security.Audit;
using System.Runtime.CompilerServices;

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
                if(!appService.ServiceProviders.Any(t=>t.Type == itm))
                {
                    appService.ServiceProviders.Add(new TypeReferenceConfiguration(itm));
                }
            }
        }
    }
}
