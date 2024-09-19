/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Configuration;
using SanteDB.Docker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SanteDB.Docker.Server.Features
{
    /// <summary>
    /// Docker feature for SMTP service
    /// </summary>
    public class SmtpDockerFeature : IDockerFeature
    {
        private const string SERVER_SETTING_NAME = "SERVER";
        private const string TLS_SETTING_NAME = "TLS";
        private const string USER_SETTING_NAME = "USER";
        private const string PASS_SETTING_NAME = "PASS";
        private const string ADMIN_SETTING_NAME = "ADMIN_EMAIL";
        private const string FROM_SETTING_NAME = "FROM";

        /// <inheritdoc/>
        public string Id => "SMTP";

        /// <inheritdoc/>
        public IEnumerable<string> Settings => new string[]
        {
            SERVER_SETTING_NAME, USER_SETTING_NAME, PASS_SETTING_NAME, TLS_SETTING_NAME, FROM_SETTING_NAME
        };

        /// <inheritdoc/>
        public void Configure(SanteDBConfiguration configuration, IDictionary<string, string> settings)
        {
            var smtpSection = configuration.GetSection<EmailNotificationConfigurationSection>();
            if (smtpSection == null)
            {
                smtpSection = new EmailNotificationConfigurationSection();
                configuration.AddSection(smtpSection);
            }

            if (!settings.TryGetValue(SERVER_SETTING_NAME, out var server))
            {
                throw new InvalidOperationException($"{SERVER_SETTING_NAME} is required for SMTP");
            }
            else if (!Uri.TryCreate(server, UriKind.Absolute, out var serverUri))
            {
                throw new InvalidCastException($"{SERVER_SETTING_NAME} must be in form smtp://[server]:[port]");
            }
            _ = settings.TryGetValue(USER_SETTING_NAME, out var user);
            _ = settings.TryGetValue(PASS_SETTING_NAME, out var pass);
            _ = settings.TryGetValue(TLS_SETTING_NAME, out var tls);
            _ = settings.TryGetValue(FROM_SETTING_NAME, out var from);

            smtpSection.Smtp = new SmtpConfiguration()
            {
                Server = server,
                From = from,
                Password = pass,
                Username = user,
                Ssl = XmlConvert.ToBoolean(tls)
            };

            if (settings.TryGetValue(ADMIN_SETTING_NAME, out var admin))
            {
                smtpSection.AdministrativeContacts = admin.Split(';').ToList();
            }
        }
    }
}
