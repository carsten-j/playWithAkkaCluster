using Akka.Actor;
using Akka.Configuration;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Cluster.Tools.Client;
using Akka.Routing;
using Shared;

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

            var cluster = ActorSystem.Create("MyCluster",
                config.WithFallback(ClusterClientReceptionist.DefaultConfig()));

            // Use PBM to manage cluster membership
            var pbm = PetabridgeCmd.Get(cluster);
            // Activate clustering commands
            pbm.RegisterCommandPalette(ClusterCommands.Instance);
            // Start Petabridge.Cmd host on 9110 (configured in HOCON)
            pbm.Start();

            var supervisorStrategy = new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromMinutes(3),
                decider: Decider.From(e =>
                {
                    if (e is DivideByZeroException ||
                        e is UnknownOperationException)
                        return Directive.Resume;
                    return Directive.Escalate;
                }));

            var distributor =
                cluster.ActorOf(
                    Props.Create(() => new Worker())
                        .WithRouter(FromConfig.Instance)
                        .WithSupervisorStrategy(supervisorStrategy), "distributor");

            ClusterClientReceptionist.Get(cluster).RegisterService(distributor);

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