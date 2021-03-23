using SanteDB.Core.Configuration;
using SanteDB.Core.Exceptions;
using SanteDB.Matcher.Configuration;
using SanteDB.Matcher.Matchers;
using SanteDB.Matcher.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// Docker feature for matching configuration
    /// </summary>
    public class MatchingFeature : IDockerFeature
    {

        // Mode setting
        public const string ModeSetting = "MODE";
        public const string SimpleMode = "SIMPLE";
        public const string WeightedMode = "WEIGHTED";

        /// <summary>
        /// Gets the id of the matching feature
        /// </summary>
        public string Id => "CORE_MATCHING";

        public IEnumerable<string> Settings => new String[] { ModeSetting };

        /// <summary>
        /// Configure the feature
        /// </summary>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            if(configuration.GetSection<FileMatchConfigurationSection>() == null) // Add configuration for file matching
            {
                configuration.AddSection(DockerFeatureUtils.LoadConfigurationResource<FileMatchConfigurationSection>("SanteDB.Docker.Core.Features.Config.MatchingFeature.xml"));
            }

            // Add the file match configuration service
            var serviceConfiguration = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            if (!serviceConfiguration.Any(s => s.Type == typeof(FileMatchConfigurationProvider)))
                serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(FileMatchConfigurationProvider)));

            // Did the hoster specify a matcher they want to use?
            if (settings.TryGetValue(ModeSetting, out string matcher))
            {
                switch (matcher)
                {
                    case SimpleMode:
                        serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(SimpleRecordMatchingService)));
                        break;
                    case WeightedMode:
                        serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(WeightedRecordMatchingService)));
                        break;
                    default:
                        throw new ConfigurationException($"{matcher} is not a valid valud for SDB_MATCHING_MODE. Use either SIMPLE or WEIGHTED", configuration);
                }
            }
            else
                serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(WeightedRecordMatchingService)));

        }
    }
}
