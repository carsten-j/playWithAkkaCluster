using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace WatchDog
{
    public class WatchDog : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger<SerilogLoggingAdapter>();

        public WatchDog()
        {
            var number = 2;

            var processes = Enumerable.Range(0, number).Select(i => new { i, Process = StartProcess() }).ToDictionary(t => t.i, t => t.Process);

            Thread.Sleep(30000);

            Receive<CheckProcesses>(_ =>
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
                    FileName = "dotnet",
                    Arguments = "worker.dll",
                    WorkingDirectory = @"C:\Users\bc0030\Downloads\playWithAkkaCluster-master\Worker\bin\Debug\netcoreapp2.1\",
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