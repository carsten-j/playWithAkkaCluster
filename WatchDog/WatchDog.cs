using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace WatchDog
{
    public class WatchDog : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger<SerilogLoggingAdapter>();

        public WatchDog()
        {
            const int number = 2;

            var waitForSeconds = new List<int>();
            waitForSeconds.Add(0);
            waitForSeconds.Add(30);

            var processes = Enumerable.Range(0, number).Select(i => new {i, Process = StartProcess(waitForSeconds[i])})
                .ToDictionary(t => t.i, t => t.Process);

            Receive<CheckProcesses>(_ =>
            {
                foreach (var process in processes.ToList())
                {
                    // restart process if it exited, e.g. by crashing
                    if (process.Value.HasExited)
                    {
                        processes[process.Key] = StartProcess(0);
                    }

                    // when the worker has been running for more than a given period
                    // then we kill the process and restart it
                    var processHasBeenRunning = DateTime.Now - process.Value.StartTime;
                    if (processHasBeenRunning.TotalMinutes > 1)
                    {
                        processes[process.Key].Kill();
                        processes[process.Key] = StartProcess(0);
                    }
                }
            });

            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), Self,
                new CheckProcesses(), Self);
        }

        private Process StartProcess(int waitSeconds)
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
            _log.Info("Starting process ...");

            Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
            process.Start();
            _log.Info("Process started ...");
            return process;
        }

        private class CheckProcesses
        { }
    }
}