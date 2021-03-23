﻿using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SanteDB.Docker.Core
{
    /// <summary>
    /// A configuration manager which reads from environment variables 
    /// </summary>
    public class DockerConfigurationManager : IConfigurationManager
    {

        // The configuration
        private SanteDBConfiguration m_configuration;

        /// <summary>
        /// Create new file confiugration service.
        /// </summary>
        public DockerConfigurationManager() : this(String.Empty)
        {

        }

        /// <summary>
        /// Get configuration service
        /// </summary>
        public DockerConfigurationManager(string configFile)
        {
            try
            {
                if (String.IsNullOrEmpty(configFile))
                    configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "santedb.config.xml");
                else if (!Path.IsPathRooted(configFile))
                    configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), configFile);

                using (var s = File.OpenRead(configFile))
                    this.m_configuration = SanteDBConfiguration.Load(s);

                var enabledFeatures = Environment.GetEnvironmentVariable(DockerConstants.EnvFeatureList)?.Split(';');
                if (enabledFeatures == null || enabledFeatures.Length == 0)
                    throw new InvalidOperationException($"No features configured - use {DockerConstants.EnvFeatureList}");

                IDictionary<String, IDockerFeature> features = new Dictionary<String, IDockerFeature>();

                // Load all assemblies into our appdomain 
                foreach(var f in Directory.GetFiles("*.dll"))
                {
                    var rfoAsm = Assembly.ReflectionOnlyLoadFrom(f);
                    if(rfoAsm.GetExportedTypes().Any(t=>t.GetInterface(typeof(IDockerFeature).FullName) != null))
                    {
                        var fAsm = Assembly.LoadFrom(f);
                        foreach(var feature in fAsm.ExportedTypes
                            .Where(t=>typeof(IDockerFeature).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                            .Select(t=>Activator.CreateInstance(t) as IDockerFeature) )
                        {
                            features.Add(feature.Id, feature);
                        }
                    }
                }

                // Now enable features
                foreach(var f in enabledFeatures)
                {
                    if (!features.TryGetValue(f, out IDockerFeature feature))
                    {
                        throw new InvalidOperationException($"Feature {f} not understood - Known features: {String.Join(",", features.Select(ft => ft.Key))}");
                    }    
                    else
                    {
                        var settings= Environment.GetEnvironmentVariables().Keys.OfType<String>().Where(d => d.StartsWith($"{DockerConstants.EnvConfigPrefix}{feature.Id}_")).ToDictionary(o => o.Replace($"{DockerConstants.EnvConfigPrefix}{feature.Id}_",""), o => Environment.GetEnvironmentVariable(o));
                        feature.Configure(this.m_configuration, settings);
                    }
                }
                
            }
            catch (Exception e)
            {
                Trace.TraceError("Error loading configuration: {0}", e);
                throw;
            }
        }

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
