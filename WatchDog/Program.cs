using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace WatchDog
{
    internal static class MainClass
    {
        public static void Main()
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

            while (true)
            {
                Thread.Sleep(2000);
                var number1 = random.Next(10);
                var number2 = random.Next(10);
                Log.Logger.Information("Adding number {Number1} and number {Number2}", number1, number2);
                var job = new CalculationJob(number1, number2, "ADD");
                worker.Tell(job);
            }
        }
    }
}