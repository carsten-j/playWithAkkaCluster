using Akka.Actor;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
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
            var config = ConfigurationFactory.ParseString(hocon);

            var actorSystem = ActorSystem.Create("MyCluster",
                config.WithFallback(ClusterClientReceptionist.DefaultConfig()));

            var worker = actorSystem.ActorOf(Props.Create(() => new WatchDog.Worker()), "worker");

            Log.Logger.Information("Actor system created");

            Log.Logger.Information("Actor system joined cluster");

            // allow process to exit when Control + C is invoked
            Console.CancelKeyPress += async (sender, e) => await CoordinatedShutdown.Get(actorSystem)
                .Run(CoordinatedShutdown.ClrExitReason.Instance).ConfigureAwait(false);

            await actorSystem.WhenTerminated;
            Log.Logger.Information("Worker actor system terminated");
        }
    }
}