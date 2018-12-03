using Akka.Actor;
using Akka.Configuration;
using Serilog;
using System;
using System.IO;

namespace Worker
{
    internal static class MainClass
    {
        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .MinimumLevel.Information()
                            .CreateLogger();

            var hocon = File.ReadAllText("worker.hocon");
            var config = ConfigurationFactory.ParseString(hocon);

            var actorSystem = ActorSystem.Create("MyCluster", config);

            var worker = actorSystem.ActorOf(Props.Create(() => new WatchDog.Worker()), "worker");

            Log.Logger.Information("Actor system created");

            Log.Logger.Information("Actor system joined cluster");

            // allow process to exit when Control + C is invoked
            Console.CancelKeyPress += (sender, e) =>
            {
                CoordinatedShutdown.Get(actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance).Wait();
            };

            actorSystem.WhenTerminated.Wait();
            Log.Logger.Information("Actor system terminated");
        }
    }
}