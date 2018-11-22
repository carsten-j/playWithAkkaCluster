using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Event;
using Akka.Logger.Serilog;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
            var actorsytem = ActorSystem.Create("MyCluster", config);
            Log.Logger.Information("Actor system created");

            Log.Logger.Information("Actor system joined cluster");

            actorsytem.WhenTerminated.Wait();
            Log.Logger.Information("Actor system terminatedß");
        }
    }
}