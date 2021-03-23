﻿using SanteDB.Core.Configuration;
using SanteDB.Core.Exceptions;
using SanteDB.Docker.Core;
using SanteDB.Messaging.Atna;
using SanteDB.Messaging.Atna.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SanteDB.Docker.Server.Features
{
    /// <summary>
    /// Enables ATNA audit shipping
    /// </summary>
    public class AtnaAuditFeature : IDockerFeature
    {

        public const string TargetSetting = "TARGET";
        public const string ModeSetting = "MODE";
        public const string SiteSetting = "SITE";
        public const string CertificateSetting = "CERT";

        /// <summary>
        /// ATNA Auditing feature
        /// </summary>
        public string Id => "AUDIT_SHIPPING";

        /// <summary>
        /// Get the settings
        /// </summary>
        public IEnumerable<string> Settings => new String[]
        {
            TargetSetting, ModeSetting, SiteSetting
        };

        /// <summary>
        /// Configure the host for ATNA audit log shipping
        /// </summary>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var atnaConfig = configuration.GetSection<AtnaConfigurationSection>();
            if (atnaConfig == null)
            {
                atnaConfig = DockerFeatureUtils.LoadConfigurationResource<AtnaConfigurationSection>("SanteDB.Docker.Server.Features.Config.AtnaAuditFeature.xml");
                configuration.AddSection(atnaConfig);
            }

            // Is the ATNA audit service available?
            var serviceConfiguration = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
            if (!serviceConfiguration.Any(s => s.Type == typeof(AtnaAuditService)))
                serviceConfiguration.Add(new TypeReferenceConfiguration(typeof(AtnaAuditService)));

            // Configure the ATNA service?
            if (settings.TryGetValue(TargetSetting, out string target))
            {
                if (!Uri.TryCreate(target, UriKind.Absolute, out Uri targetUri))
                {
                    throw new ConfigurationException($"Target {target} is not a valid URI", configuration);
                }

                atnaConfig.AuditTarget = $"{targetUri.Host}:{targetUri.Port}";
                if (!Enum.TryParse<AtnaTransportType>(targetUri.Scheme, true, out AtnaTransportType transport))
                {
                    throw new ConfigurationException($"Scheme {targetUri.Scheme} not recognized", configuration);
                }
                atnaConfig.Transport = transport;
            }

            if(settings.TryGetValue(ModeSetting, out string mode))
            {
                if(!Enum.TryParse< AtnaApi.Transport.MessageFormatType>(mode, true, out AtnaApi.Transport.MessageFormatType format))
                {
                    throw new ConfigurationException($"Format {mode} is not understood", configuration);
                }
                atnaConfig.Format = format;
            }

            if(settings.TryGetValue(SiteSetting, out string site))
            {
                atnaConfig.EnterpriseSiteId = site;
            }

            if(settings.TryGetValue(CertificateSetting, out string certThumbprint))
            {
                atnaConfig.ClientCertificate = new SanteDB.Core.Security.Configuration.X509ConfigurationElement()
                {
                    FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint,
                    FindValue = certThumbprint,
                    StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    StoreName = System.Security.Cryptography.X509Certificates.StoreName.My
                };
            }
        }
    }
}
