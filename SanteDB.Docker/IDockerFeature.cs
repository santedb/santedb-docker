using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Docker
{
    /// <summary>
    /// Represents a grouping of features which can be configured via Docker environment variable
    /// </summary>
    /// <remarks>
    /// This interface allows a plugin to expose one or more services via the SDB_ENABLE=x,y,z environment variable.
    /// </remarks>
    public interface IDockerFeature
    {

        /// <summary>
        /// Gets the identifier of the feature
        /// </summary>
        /// <remarks>This is the identifier that appears in the SDB_FEATURE</remarks>
        string Id { get; }

        /// <summary>
        /// Configure the feature for execution in Docker
        /// </summary>
        /// <remarks>Allows the feature to be configured against the provided <paramref name="configuration"/></remarks>
        /// <param name="configuration">The configuration into which the feature should be configured</param>
        /// <param name="settings">Settings which were parsed from the environment in SDB_{this.Id}_{Key}={Value}</param>
        bool Configure(SanteDBConfiguration configuration, IDictionary<String, String> settings);
    }
}
