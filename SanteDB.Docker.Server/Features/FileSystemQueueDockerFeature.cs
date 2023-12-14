using SanteDB.Core.Configuration;
using SanteDB.Core.PubSub.Broker;
using SanteDB.Docker.Core;
using SanteDB.Persistence.PubSub.ADO.Configuration;
using SanteDB.Persistence.PubSub.ADO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Services.Impl;

namespace SanteDB.Docker.Server.Features
{
    /// <summary>
    /// A docker feature which registers the file system queue 
    /// </summary>
    public class FileSystemQueueDockerFeature : IDockerFeature
    {
        /// <inheritdoc/>
        public string Id => "FS_MQ";

        /// <inheritdoc/>
        public IEnumerable<string> Settings => new String[0];

        /// <inheritdoc/>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {

            // Add service for persisting
            var serviceConfiguration = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            if (!serviceConfiguration.Any(s => s.Type == typeof(FileSystemDispatcherQueueService)))
            {
                serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(FileSystemDispatcherQueueService)));
            }
        }
    }
}
