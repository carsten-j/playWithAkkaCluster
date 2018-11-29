using System;
using Akka.Actor;
using Akka.Configuration;
using Serilog;
using System.IO;
using Petabridge.Cmd;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;

namespace SeedNode
{
    internal static class MainClass
    {
        // dotnet run -- 4053
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .MinimumLevel.Information()
                            .CreateLogger();

            var hocon = File.ReadAllText("seedNode.hocon");
            var config = ConfigurationFactory.ParseString(hocon);

            var cluster = ActorSystem.Create("MyCluster", config);

            // use PBM to manage cluster membership
            var pbm = PetabridgeCmd.Get(cluster);
            pbm.RegisterCommandPalette(ClusterCommands.Instance); // activate clustering commands
            pbm.Start(); // start Petabridge.Cmd host on 9110 (configured in HOCON)

            // allow process to exit when Control + C is invoked
            Console.CancelKeyPress += (sender, e) =>
            {
                CoordinatedShutdown.Get(cluster).Run(CoordinatedShutdown.ClrExitReason.Instance);
            };

            cluster.WhenTerminated.Wait();
        }
    }
}