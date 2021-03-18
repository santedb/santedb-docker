using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Docker
{
    /// <summary>
    /// A configuration manager which reads from environment variables 
    /// </summary>
    public class DockerConfigurationManager : IConfigurationManager
    {

        // The configuration
        private SanteDBConfiguration m_configuration;

        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public SanteDBConfiguration Configuration => this.m_configuration;

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Docker Configuration Manager";

        /// <summary>
        /// Get an application setting
        /// </summary>
        public string GetAppSetting(string key)
        {
            var retVal = Environment.GetEnvironmentVariable(key);
            if (String.IsNullOrEmpty(retVal))
                retVal = this.m_configuration.GetSection<ApplicationServiceContextConfigurationSection>().AppSettings.Find(o => o.Key == key)?.Value;
            return retVal;
        }

        /// <summary>
        /// Gets the specified configuration string
        /// </summary>
        public ConnectionString GetConnectionString(string key)
        {
            var retVal = Environment.GetEnvironmentVariable($"{DockerConstants.EnvConnectionStringPrefix}_{key.ToUpper()}");
            if (String.IsNullOrEmpty(retVal))
                return this.m_configuration.GetSection<DataConfigurationSection>().ConnectionString.Find(o => o.Name == key);
            else {
                var objData = retVal.Split(';').Select(o => o.Split('=')).ToDictionary(o => o[0], o => (object)o[1]);
                if (objData.TryGetValue("_provider", out object pvdr))
                    return new ConnectionString(pvdr.ToString(), objData);
                else
                    throw new ConfigurationException($"No provider configurated for {key}", this.m_configuration);
            }
        }

        /// <summary>
        /// Get the specified configuration section
        /// </summary>
        public T GetSection<T>() where T : IConfigurationSection
        {
            return this.m_configuration.GetSection<T>();
        }

        /// <summary>
        /// Reload the configuration
        /// </summary>
        public void Reload()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets an app setting
        /// </summary>
        public void SetAppSetting(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
