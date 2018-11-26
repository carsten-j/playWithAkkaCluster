using System;
using Akka.Actor;
using System.Diagnostics;
using System.Linq;
using Akka.Cluster;
using Serilog;
using Akka.Logger.Serilog;
using Akka.Event;
using System.Threading;

namespace WatchDog
{
    public class WatchDog : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger<SerilogLoggingAdapter>();

        public WatchDog()
        {
            var number = 1;

            var processes = Enumerable.Range(0, number).Select(i => new {i, Process = StartProcess()}).ToDictionary(t => t.i, t => t.Process);
            
            Thread.Sleep(30000);

            Receive<CheckProcesses>(m =>
            {
                foreach (var process in processes.ToList())
                {
                    if (process.Value.HasExited)
                    {
                        processes[process.Key] = StartProcess();
                    }
                }
            });
            
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Self,
                new CheckProcesses(), Self);                
        }

        private Process StartProcess()
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "worker",
                    WorkingDirectory = @"/Users/carsten/Projects/playWithAkkaCluster/Worker/bin/Debug/netcoreapp2.1/osx-x64/publish/",
                    UseShellExecute = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false
                }
            };
            log.Info("Starting process ...");

            process.Start();
            log.Info("Process started ...");
            return process;
        }

        private class CheckProcesses
        { }            
    }
}
