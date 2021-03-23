using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Docker.Core
{
    /// <summary>
    /// Docker feature utility
    /// </summary>
    public static class DockerFeatureUtils
    {

        /// <summary>
        /// Load default configuration section from a resource manifest stream
        /// </summary>
        public static TConfiguration LoadConfigurationResource<TConfiguration>(String resourceName) where TConfiguration : IConfigurationSection
        {
            using (var str = typeof(DockerFeatureUtils).Assembly.GetManifestResourceStream(resourceName))
            {
                var config = SanteDBConfiguration.Load(str);
                return config.GetSection<TConfiguration>();
            }
        }
    }
}
