using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            if(dxConfig == null)
            {
                dxConfig = DockerFeatureUtils.LoadConfigurationResource<DiagnosticsConfigurationSection>("SanteDB.Docker.Core.Features.Config.DiagnosticsFeature.xml");
                configuration.AddSection(dxConfig);
            }

            // Set the log level
            if(settings.TryGetValue(LogLevelSettings, out String level))
            {
                if(!Enum.TryParse< System.Diagnostics.Tracing.EventLevel>(level, out System.Diagnostics.Tracing.EventLevel filter))
                {
                    throw new ArgumentOutOfRangeException($"Expected values for {this.Id}_{LogLevelSettings} are Informational, Warning, Error, LogAlways");
                }
                dxConfig.TraceWriter.ForEach(o => o.Filter = filter);
            }

        }
    }
}
