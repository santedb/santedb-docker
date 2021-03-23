using SanteDB.Core.Configuration;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Privacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// Privacy filtering 
    /// </summary>
    public class PrivacyFilterFeature : IDockerFeature
    {

        /// <summary>
        /// Resources to be protected
        /// </summary>
        public const string ResourceTypeSetting = "RESOURCE";
        /// <summary>
        /// Action to take
        /// </summary>
        public const string ActionSetting = "ACTION";

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
            if(settings.TryGetValue(ActionSetting, out string action)) {
                if(!Enum.TryParse< ResourceDataPolicyActionType>(action, true, out ResourceDataPolicyActionType actionEnum))
                {
                    throw new ArgumentOutOfRangeException($"{action} is not recognized as an action (valid: NONE, AUDIT, REDACT, NULLIFY, HIDE, ERROR, HASH)");
                }
                privacyConf.DefaultAction = actionEnum;
            }

            if(settings.TryGetValue(ResourceTypeSetting, out string resources))
            {
                privacyConf.Resources = resources.Split(',').Select(o => new ResourceDataPolicyFilter()
                {
                    ResourceTypeXml = o,
                    Action = privacyConf.DefaultAction
                }).ToList();
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
