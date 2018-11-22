using System;
using Serilog;
using Akka.Actor;
using System.IO;
using Akka.Configuration;
using Akka.Routing;
using System.Threading;
using Akka.Cluster.Routing;

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

            Serilog.Log.Information("watchdog created");

            var worker = cluster.ActorOf(Props.Create(() => new Worker()).WithRouter(FromConfig.Instance), "worker");

            Serilog.Log.Information("worker router created");

            while (true)
            {
                Console.WriteLine("Enter first number ...");
                var number1 = Console.ReadLine();
                Console.WriteLine("Enter second number ...");
                var number2 = Console.ReadLine();
                Console.WriteLine("Enter operation ...");
                var opr = Console.ReadLine();
                var job = new CalculationJob(int.Parse(number1), int.Parse(number2), opr);
                worker.Tell(job);
            }
        }
    }
}