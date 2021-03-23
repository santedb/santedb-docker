using SanteDB.Caching.Memory;
using SanteDB.Caching.Memory.Configuration;
using SanteDB.Caching.Redis;
using SanteDB.Caching.Redis.Configuration;
using SanteDB.Core.Configuration;
using SanteDB.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// Caching feature
    /// </summary>
    public class CachingFeature : IDockerFeature
    {


        public const string ModeRedis = "REDIS";
        public const string ModeMemory = "LOCAL";
        public const string MaxAgeSetting = "EXPIRE";
        public const string RedisAddressSetting = "REDIS_SERVER";
        public const string ModeSetting = "MODE";

        /// <summary>
        /// Get the id of this feature
        /// </summary>
        public string Id => "CACHE";

        /// <summary>
        /// Settings for the caching feature
        /// </summary>
        public IEnumerable<string> Settings => new String[] { ModeSetting, MaxAgeSetting, RedisAddressSetting };

        /// <summary>
        /// Configure this service
        /// </summary>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {

            if(!settings.TryGetValue(ModeSetting, out string mode))
            {
                throw new ConfigurationException($"{this.Id}_{ModeSetting} must be specified", configuration);
            }
            else
            {

                // The type of service to add
                Type[] serviceTypes = null;

                if (!settings.TryGetValue(MaxAgeSetting, out string maxAge)) {
                    maxAge = "PT1H";
                }

                // Parse
                if (!TimeSpan.TryParse(maxAge, out TimeSpan maxAgeTs))
                {
                    throw new ConfigurationException($"{maxAge} is not understood as a timespan", configuration);
                }

                switch(mode.ToUpperInvariant())
                {
                    case ModeRedis:

                        var redisSetting = configuration.GetSection<RedisConfigurationSection>();
                        if (redisSetting == null)
                        {
                            redisSetting = DockerFeatureUtils.LoadConfigurationResource<RedisConfigurationSection>("SanteDB.Docker.Core.Features.Config.RedisCacheFeature.xml");
                            configuration.AddSection(redisSetting);
                        }
                        redisSetting.TTLXml = maxAgeTs.ToString();

                        if(settings.TryGetValue(RedisAddressSetting, out string redisServer))
                        {
                            redisSetting.Servers = new List<string>() { redisServer };
                        }
                        serviceTypes = new Type[] {
                            typeof(RedisCacheService),
                            typeof(RedisAdhocCache),
                            typeof(RedisQueryPersistenceService)
                        };

                        break;
                    case ModeMemory:

                        var memSetting = configuration.GetSection<MemoryCacheConfigurationSection>();
                        if(memSetting == null)
                        {
                            memSetting = DockerFeatureUtils.LoadConfigurationResource<MemoryCacheConfigurationSection>("SanteDB.Docker.Core.Features.Config.MemCacheFeature.xml");
                            configuration.AddSection(memSetting);
                        }
                        memSetting.MaxQueryAge = memSetting.MaxCacheAge = (long)maxAgeTs.TotalSeconds;
                        serviceTypes = new Type[] {
                            typeof(MemoryCacheService),
                            typeof(MemoryAdhocCacheService),
                            typeof(MemoryQueryPersistenceService)
                        };

                        break;
                    default:
                        throw new ConfigurationException($"Mode {mode} is not understood", configuration);
                }

                // Add services
                var serviceConfiguration = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
                serviceConfiguration.AddRange(serviceTypes.Where(t => !serviceConfiguration.Any(c => c.Type == t)).Select(t => new TypeReferenceConfiguration(t)));
            }
        }
    }
}
