using Akka.Actor;
using Akka.Configuration;
using Serilog;
using System;
using System.IO;

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
            
            cluster.WhenTerminated.Wait();
        }
    }
}