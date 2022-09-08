/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using Mono.Unix;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.Docker.Core;
using SanteDB.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SanteDB.Docker.Server
{
    /// <summary>
    /// Docker host
    /// </summary>
    /// <remarks>This host is minimalist implementation of a host which just starts up and
    /// shuts down when the docker instance is closed</remarks>
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("SanteDB iCDR Server for Docker and Kubernetes v.{0} ({1})", typeof(Program).Assembly.GetName().Version, typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
                Console.WriteLine("{0}", typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);
#if DEBUG
                // Minimum services for startup
                if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("SDB_FEATURE")))
                {
                    Environment.SetEnvironmentVariable("SDB_FEATURE", "RAMCACHE;ADO;PUBSUB_ADO;SEC;LOG;FHIR;HL7;HDSI;AMI;BIS;SWAGGER;AUDIT_REPO;OPENID");
                    Environment.SetEnvironmentVariable("SDB_DB_AUDIT", "server=sdb-postgres; database=audit; user=santedb; password=SanteDB123");
                    Environment.SetEnvironmentVariable("SDB_DB_MAIN", "server=sdb-postgres; database=audit; user=santedb; password=SanteDB123");
                    Environment.SetEnvironmentVariable("SDB_DB_MAIN_PROVIDER", "Npgsql");
                    Environment.SetEnvironmentVariable("SDB_DB_AUDIT_PROVIDER", "Npgsql");
                }
#endif

                // Install certs
                try
                {
                    Console.WriteLine("Installing Security Certificiates...");
                    SecurityExtensions.InstallCertsForChain();
                }
                catch
                {
                    Console.WriteLine("Unable to install security certificates - are you running on Docker and as ROOT?");
                }
                // Wait ?
                // HACK: Docker doesn't wait for other services to come up
                var wait = Environment.GetEnvironmentVariable("SDB_DELAY_START");
                if (!String.IsNullOrEmpty(wait) && Int32.TryParse(wait, out int waitInt))
                {
                    Console.WriteLine("Waiting for {0} ms before start...", waitInt);
                    Thread.Sleep(waitInt);
                    Console.WriteLine("Continuing startup...");
                }
                IConfigurationManager configurationManager = null;

                if (args?.Length == 1) // User passed an alternate configuration file
                    configurationManager = new DockerConfigurationManager(args[0]);
                else
                    configurationManager = new DockerConfigurationManager();

                // Start the host process
                ServiceUtil.Start(Guid.NewGuid(), configurationManager);

                Console.WriteLine("Service startup completed...");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Wait until cancel key is pressed
                    var mre = new ManualResetEventSlim(false);
                    Console.CancelKeyPress += (o, e) =>
                    {
                        Console.WriteLine("Sending request to shutdown...");
                        ServiceUtil.Stop();
                        mre.Set();
                    };
                    mre.Wait();
                }
                else
                {  // running on unix
                   // Now wait until the service is exiting va SIGTERM or SIGSTOP
                    UnixSignal[] signals = new UnixSignal[]
                    {
                new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
        new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
        new UnixSignal(Mono.Unix.Native.Signum.SIGQUIT),
        new UnixSignal(Mono.Unix.Native.Signum.SIGHUP)
                    };
                    int signal = UnixSignal.WaitAny(signals);
                    // Gracefully shutdown
                    ServiceUtil.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FATAL ERROR: HALTING SANTEDB HOST PROCESS : {0}", e);
            }
        }
    }
}