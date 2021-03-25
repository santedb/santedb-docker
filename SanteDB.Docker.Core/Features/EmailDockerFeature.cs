using SanteDB.Core.Configuration;
using SanteDB.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// E-Mail sender feature
    /// </summary>
    public class EmailDockerFeature : IDockerFeature
    {
        /// <summary>
        /// SMTP Server
        /// </summary>
        public const string SmtpSetting = "SMTP";

        /// <summary>
        /// Sets the FROM address
        /// </summary>
        public const string FromAddressSetting = "FROM";

        /// <summary>
        /// Administrative setting
        /// </summary>
        public const string AdminContactSetting = "ADMIN";

        /// <summary>
        /// The id of the feature
        /// </summary>
        public string Id => "EMAIL";

        /// <summary>
        /// Settings supported
        /// </summary>
        public IEnumerable<string> Settings => new String[] { SmtpSetting, FromAddressSetting, AdminContactSetting };

        /// <summary>
        /// Configure the feature
        /// </summary>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var emailConf = configuration.GetSection<EmailNotificationConfigurationSection>();
            if (emailConf == null)
            {
                emailConf = new EmailNotificationConfigurationSection();
                configuration.AddSection(emailConf);
            }

            // Action settings
            if (settings.TryGetValue(SmtpSetting, out string smtpString))
            {
                if(!Uri.TryCreate(smtpString, UriKind.Absolute, out Uri smtpUri))
                {
                    throw new ArgumentException($"Format of SMTP setting is: smtp[s]://user:pass@server:port");
                }

                var userInfo = smtpUri.UserInfo.Split(':');
                emailConf.Smtp = new SmtpConfiguration()
                {
                    Password = userInfo.Length > 1 ? userInfo[1] : null,
                    Username = userInfo.Length > 0 ? userInfo[0] : null,
                    Server = $"smtp://{smtpUri.Host}:{smtpUri.Port}",
                    Ssl = smtpUri.Scheme == "smtps"
                };
            }
            else if(emailConf.Smtp == null)
            {
                throw new ConfigurationException("SMTP configuration missing", configuration);
            }


            if (settings.TryGetValue(FromAddressSetting, out string fromAddress))
            {
                emailConf.Smtp.From = fromAddress;
            }

            if(settings.TryGetValue(AdminContactSetting, out string admins))
            {
                emailConf.AdministrativeContacts = admins.Split(';').ToList();
            }

        }
    }
}
