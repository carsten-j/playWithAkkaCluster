using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Worker
{
    internal static class MainClass
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .MinimumLevel.Information()
                            .CreateLogger();

            var hocon = await File.ReadAllTextAsync("worker.hocon").ConfigureAwait(false);
            var random = new Random();
            var port = 9200 + random.Next(1, 50);
            hocon = hocon.Replace("{PORT}", port.ToString());
            var config = ConfigurationFactory.ParseString(hocon);

            var cluster = ActorSystem.Create("MyCluster",
                config.WithFallback(ClusterClientReceptionist.DefaultConfig()));

            // Use PBM to manage cluster membership
            var pbm = PetabridgeCmd.Get(cluster);
            // Activate clustering commands
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            // Start Petabridge.Cmd host on 9112 (configured in HOCON)
            pbm.Start();

            Log.Logger.Information("Actor system created");

            Log.Logger.Information("Actor system joined cluster");

            // allow process to exit when Control + C is invoked
            Console.CancelKeyPress += async (sender, e) => await CoordinatedShutdown.Get(cluster)
                .Run(CoordinatedShutdown.ClrExitReason.Instance).ConfigureAwait(false);

            await cluster.WhenTerminated;
            Log.Logger.Information("Worker actor system terminated");
        }
    }
}