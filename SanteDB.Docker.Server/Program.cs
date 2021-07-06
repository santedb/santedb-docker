using Mono.Unix;
using SanteDB.Core.Services;
using SanteDB.Docker.Core;
using SanteDB.Server;
using System;
using System.Collections.Generic;
using System.Linq;
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
    class Program
    {
        static void Main(string[] args)
        {

            try
            {

#if DEBUG
            // Minimum services for startup
            if(String.IsNullOrEmpty(Environment.GetEnvironmentVariable("SDB_FEATURE")))
            {
                Environment.SetEnvironmentVariable("SDB_FEATURE", "RAMCACHE;ADO;PUBSUB_ADO;SEC;LOG;FHIR;HL7;HDSI;AMI;BIS;SWAGGER;AUDIT_REPO;OPENID");
                Environment.SetEnvironmentVariable("SDB_DB_AUDIT", "server=sdb-postgres; database=audit; user=santedb; password=SanteDB123");
                Environment.SetEnvironmentVariable("SDB_DB_MAIN", "server=sdb-postgres; database=audit; user=santedb; password=SanteDB123");
                    Environment.SetEnvironmentVariable("SDB_DB_MAIN_PROVIDER", "Npgsql");
                    Environment.SetEnvironmentVariable("SDB_DB_AUDIT_PROVIDER", "Npgsql");
                }
#endif
                // Wait ?
                // HACK: Docker doesn't wait for other services to come up
                var wait = Environment.GetEnvironmentVariable("SDB_DELAY_START");
                if(!String.IsNullOrEmpty(wait) && Int32.TryParse(wait, out int waitInt))
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

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Wait until cancel key is pressed
                    var mre = new ManualResetEventSlim(false);
                    Console.CancelKeyPress += (o, e) => mre.Set();
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
                }

                // Gracefully shutdown
                ServiceUtil.Stop();
            }
            catch(Exception e)
            {
                Console.WriteLine("FATAL ERROR: HALTING SANTEDB HOST PROCESS : {0}", e);
            }

        }

    }
}
