using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Akka.Routing;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Serilog;
using Shared;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WatchDog
{
    internal static class MainClass
    {
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .MinimumLevel.Information()
                            .CreateLogger();

            var hocon = await File.ReadAllTextAsync("watchDog.hocon").ConfigureAwait(false);
            var config = ConfigurationFactory.ParseString(hocon);

            var cluster = ActorSystem.Create("MyCluster", config);

            // Use PBM to manage cluster membership
            var pbm = PetabridgeCmd.Get(cluster);
            // Activate clustering commands
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            // Start Petabridge.Cmd host on 9111 (configured in HOCON)
            pbm.Start();

            //var watchdog = cluster.ActorOf(ClusterSingletonManager.Props(
            //    singletonProps: Props.Create(() => new WatchDog()),
            //    terminationMessage: PoisonPill.Instance,
            //    settings: ClusterSingletonManagerSettings.Create(cluster).WithRole("watchdog")),
            //    name: "watchdog");

            var watchDog = cluster.ActorOf(Props.Create(() => new WatchDog()), "watchdog");

            Log.Logger.Information("worker router created");

            // allow process to exit when Control + C is invoked
            Console.CancelKeyPress += async (sender, e) =>
            {
                await CoordinatedShutdown.Get(cluster).Run(CoordinatedShutdown.ClrExitReason.Instance)
                    .ConfigureAwait(false);
            };

            // don't terminate process unless this node is downed or Control + C is invoked.
            await cluster.WhenTerminated;
            Log.Logger.Information("Watchdog actor system terminated");
        }
    }
}