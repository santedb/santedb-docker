/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;

namespace SanteDB.Docker.Core
{
    /// <summary>
    /// Represents a grouping of features which can be configured via Docker environment variable
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface allows a plugin to expose one or more services via the <c>SDB_ENABLE=x,y,z</c> environment variable. When the docker host
    /// encounters the <see cref="Id"/> in <c>SDB_ENABLE</c> it calls the <see cref="Configure(SanteDBConfiguration, IDictionary{string, string})"/> method.
    /// </para>
    /// </remarks>
    /// <example lang="C#">
    /// <![CDATA[
    /// // This feature is enabled when the user sets SDB_FEATURE=my
    /// public class MyDockerFeature : IDockerFeature
    /// {
    ///     // identifier which appears in the SDB_FEATURE list
    ///     public string Id => "my";
    ///     // settings which this feature accepts in format my_setting
    ///     public IEnumerable<String> Settings => new String[] { "setting" };
    ///     // Perform the configuration
    ///     public void Configure(SanteDBConfiguration configuration, IDictionary<String, String> settings) {
    ///         Console.WriteLine("Thanks for using my docker feature plugin!");
    ///         if(settings.TryGetValue("setting", out String setting) {
    ///             Console.WriteLine("You've used the setting {0}", setting);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    public interface IDockerFeature
    {
        /// <summary>
        /// Gets the identifier of the feature
        /// </summary>
        /// <remarks>This is the identifier that appears in the <c>SDB_FEATURE</c></remarks>
        string Id { get; }

        /// <summary>
        /// Get a list of settings allowed for this object
        /// </summary>
        /// <remarks>This enumerable is used to validate the settings which the user has passed on the environment for the
        /// host context. Each setting is prefixed with <see cref="Id"/> so, if a feature has <see cref="Id"/> of "my"
        /// and a setting of "setting" then the interpreter will look for environment variable <c>sdb_my_setting</c> and
        /// will place it into the settings parameter passed to the <see cref="Configure(SanteDBConfiguration, IDictionary{string, string})"/>
        /// method</remarks>
        IEnumerable<String> Settings { get; }

        /// <summary>
        /// Configure the feature for execution in Docker
        /// </summary>
        /// <remarks>Allows the feature to be configured against the provided <paramref name="configuration"/></remarks>
        /// <param name="configuration">The configuration into which the feature should be configured</param>
        /// <param name="settings">Settings which were parsed from the environment in SDB_{this.Id}_{Key}={Value}</param>
        void Configure(SanteDBConfiguration configuration, IDictionary<String, String> settings);
    }
}