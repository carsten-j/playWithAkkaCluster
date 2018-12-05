using Akka.Actor;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SeedNode
{
    internal static class MainClass
    {
        // dotnet run -- 4053
        public static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .MinimumLevel.Information()
                            .CreateLogger();

            var hocon = await File.ReadAllTextAsync("seedNode.hocon").ConfigureAwait(false);
            var config = ConfigurationFactory.ParseString(hocon);

            var cluster = ActorSystem.Create("MyCluster", config);

            // Use PBM to manage cluster membership
            var pbm = PetabridgeCmd.Get(cluster);
            // Activate clustering commands
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            // Start Petabridge.Cmd host on 9110 (configured in HOCON)
            pbm.Start();

            // Allow process to exit when Control + C is invoked
            Console.CancelKeyPress += async (sender, e) =>
            {
                await CoordinatedShutdown.Get(cluster).Run(CoordinatedShutdown.ClrExitReason.Instance)
                    .ConfigureAwait(false);
            };

            await cluster.WhenTerminated;
            Log.Logger.Information("Seed actor system terminated");
        }
    }
}