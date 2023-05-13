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
 * Date: 2023-3-10
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Data;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SanteDB.Docker.Core
{
    /// <summary>
    /// A configuration manager which constructs a <see cref="SanteDBConfiguration"/> from environment variables
    /// </summary>
    /// <remarks>
    /// <para>This implementation of the <see cref="IConfigurationManager"/> uses environment variables passed from a 
    /// <see href="https://help.santesuite.org/installation/installation/santedb-server/installation-using-appliances/docker-containers">Dockerized Installation Environment</see> so 
    /// that SanteDB modules may operate as though they were configured from a static configuration file. </para>
    /// <para>This class scans the <c>SDB_FEATURE</c> environment variables and locates the <see cref="IDockerFeature"/> implementation 
    /// for the specified environment variable. It then calls the <c>Configure()</c> method on those implementations and builds
    /// an instance of the <see cref="SanteDBConfiguration"/> object based on those providers.</para>
    /// <para>For more information about the Docker features and their configuration see the <see href="https://help.santesuite.org/installation/installation/santedb-server/installation-using-appliances/docker-containers/feature-configuration">Docker Feature documentation</see> article</para>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class DockerConfigurationManager : IConfigurationManager
    {

        // The configuration
        private SanteDBConfiguration m_configuration;
        private readonly ConcurrentDictionary<String, ConnectionString> m_transientConnectionStrings = new ConcurrentDictionary<string, ConnectionString>();

        /// <summary>
        /// Get configuration service
        /// </summary>
        public DockerConfigurationManager(string configFile)
        {
            try
            {
                if (String.IsNullOrEmpty(configFile))
                {
                    configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "santedb.config.xml");
                }
                else if (!Path.IsPathRooted(configFile))
                {
                    configFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), configFile);
                }

                using (var s = File.OpenRead(configFile))
                {
                    this.m_configuration = SanteDBConfiguration.Load(s);
                }

                var enabledFeatures = $"CORE;{Environment.GetEnvironmentVariable($"{DockerConstants.EnvFeatureList}")}".Split(';');
                if (enabledFeatures == null || enabledFeatures.Length == 0)
                {
                    throw new InvalidOperationException($"No features configured - use {DockerConstants.EnvFeatureList}");
                }

                IDictionary<String, IDockerFeature> features = new Dictionary<String, IDockerFeature>();

                // Load all assemblies into our appdomain 
                var scanFiles = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll").ToList();
                scanFiles.Add(Assembly.GetEntryAssembly().Location);
                foreach (var f in scanFiles)
                {
                    try
                    {
                        Console.WriteLine("Loading {0}", f);
                        var rfoAsm = Assembly.LoadFile(f);
                        if (rfoAsm.GetExportedTypes().Any(t => t.GetInterface(typeof(IDockerFeature).FullName) != null))
                        {
                            var fAsm = Assembly.LoadFrom(f);
                            foreach (var feature in fAsm.GetExportedTypesSafe()
                                .Where(t => typeof(IDockerFeature).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                                .Select(t => Activator.CreateInstance(t) as IDockerFeature))
                            {
                                features.Add(feature.Id, feature);
                            }
                        }
                    }
                    catch // ignore
                    {

                    }
                }

                // Now enable features
                foreach (var f in enabledFeatures)
                {
                    if (!features.TryGetValue(f, out IDockerFeature feature))
                    {
                        throw new InvalidOperationException($"Feature {f} not understood - Known features: {String.Join(",", features.Select(ft => ft.Key))}");
                    }
                    else
                    {
                        var settings = Environment.GetEnvironmentVariables().Keys.OfType<String>().Where(d => d.StartsWith($"{DockerConstants.EnvConfigPrefix}{feature.Id}_")).ToDictionary(o => o.Replace($"{DockerConstants.EnvConfigPrefix}{feature.Id}_", ""), o => Environment.GetEnvironmentVariable(o));
                        feature.Configure(this.m_configuration, settings);
                    }
                }

                // Attempt to write out diagnostic log
                using (var fs = File.Create($"docker.lastconfig"))
                {
                    this.m_configuration.Save(fs);
                    fs.Flush();
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

        /// <inheritdoc/>
        public bool IsReadonly => true;

        /// <summary>
        /// Get an application setting
        /// </summary>
        public string GetAppSetting(string key)
        {
            var retVal = Environment.GetEnvironmentVariable(key);
            if (String.IsNullOrEmpty(retVal))
            {
                retVal = this.m_configuration.GetSection<ApplicationServiceContextConfigurationSection>().AppSettings.Find(o => o.Key == key)?.Value;
            }

            return retVal;
        }

        /// <summary>
        /// Gets the specified configuration string
        /// </summary>
        public ConnectionString GetConnectionString(string key)
        {
            var envVar = Environment.GetEnvironmentVariable($"{DockerConstants.EnvConnectionStringPrefix}{key.ToUpper().Replace(".", "_")}");
            if (String.IsNullOrEmpty(envVar))
            {
                var retVal = this.m_configuration.GetSection<DataConfigurationSection>().ConnectionString.Find(o => o.Name == key);
                if (retVal == null)
                {
                    this.m_transientConnectionStrings.TryGetValue(key, out retVal);
                }
                return retVal;
            }
            else
            {
                var provider = Environment.GetEnvironmentVariable($"{DockerConstants.EnvConnectionStringPrefix}{key.ToUpper()}_PROVIDER");

                if (String.IsNullOrEmpty(provider))
                {
                    Environment.GetEnvironmentVariable($"{DockerConstants.EnvConnectionStringPrefix}_PROVIDER");
                }

                if (!String.IsNullOrEmpty(provider))
                {
                    return new ConnectionString(provider.ToString(), envVar);
                }
                else
                {
                    throw new ConfigurationException($"No provider configurated for {key}", this.m_configuration);
                }
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
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        /// <exception cref="NotSupportedException"></exception>
        public void SaveConfiguration()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets an app setting
        /// </summary>
        public void SetAppSetting(string key, string value)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        /// <inheritdoc/>
        public void SetTransientConnectionString(string key, ConnectionString connectionString)
        {
            if (Configuration.GetSection<DataConfigurationSection>()?.ConnectionString.Any(o => o.Name == key) == true)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DUPLICATE_OBJECT, key));
            }
            this.m_transientConnectionStrings.AddOrUpdate(key, connectionString, (k, o) => o);
        }

    }
}
