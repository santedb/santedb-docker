using SanteDB.Core;
using SanteDB.Docker.Core;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SanteDB.Docker.Server
{
    /// <summary>
    /// Docker server context
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class DockerServerContext : SanteDBContextBase
    {

        /// <summary>
        /// Create a docker service context
        /// </summary>
        internal DockerServerContext(String configurationFile) : base(SanteDBHostType.Server, new DockerConfigurationManager(configurationFile))
        {
        }
    }
}
