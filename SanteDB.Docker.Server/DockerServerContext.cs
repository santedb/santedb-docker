using SanteDB.Core;
using SanteDB.Core.Services;
using SanteDB.Docker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Docker.Server
{
    /// <summary>
    /// Docker server context
    /// </summary>
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
