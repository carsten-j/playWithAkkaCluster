using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Cluster;

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

            var hocon = File.ReadAllText("watchDog.hocon");
            var config = ConfigurationFactory.ParseString(hocon);

            var cluster = ActorSystem.Create("MyCluster", config);

            var worker = cluster.ActorOf(Props.Create(() => new Worker()).WithRouter(FromConfig.Instance), "worker");

            Log.Logger.Information("worker router created");

            var random = new Random();

            // in combination with the akka.cluster.role.frontend.min-nr-of-members = 1 setting,
            // ensures that we don't start distributing work until at least 1 worker has joined
            Cluster.Get(cluster).RegisterOnMemberUp(() =>
            {
                var recurringTask = cluster.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2),
                    () =>
                    {
                        var number1 = random.Next(10);
                        var number2 = random.Next(10);
                        Log.Logger.Information("Adding number {Number1} and number {Number2}", number1, number2);
                        var job = new CalculationJob(number1, number2, "ADD");
                        worker.Tell(job);
                    });
            });
           

            // allow process to exit when Control + C is invoked
            Console.CancelKeyPress += (sender, e) =>
                {
                    CoordinatedShutdown.Get(cluster).Run(CoordinatedShutdown.ClrExitReason.Instance).Wait();
                };

            // don't terminate process unless this node is downed or Control + C is invoked.
            await cluster.WhenTerminated;
        }

    }
}