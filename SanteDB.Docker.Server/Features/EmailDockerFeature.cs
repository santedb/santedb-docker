/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Portions Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
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
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SanteDB.Docker.Core.Features
{
    /// <summary>
    /// E-Mail sender feature
    /// </summary>
    [ExcludeFromCodeCoverage]
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
                if (!Uri.TryCreate(smtpString, UriKind.Absolute, out Uri smtpUri))
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
            else if (emailConf.Smtp == null)
            {
                throw new ConfigurationException("SMTP configuration missing", configuration);
            }


            if (settings.TryGetValue(FromAddressSetting, out string fromAddress))
            {
                emailConf.Smtp.From = fromAddress;
            }

            if (settings.TryGetValue(AdminContactSetting, out string admins))
            {
                emailConf.AdministrativeContacts = admins.Split(';').ToList();
            }

        }
    }
}
