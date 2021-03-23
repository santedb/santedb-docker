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

    }
}
