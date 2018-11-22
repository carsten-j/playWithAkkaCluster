using System;
using System.IO;
using System.Threading;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.Client;
using Akka.Configuration;
using Akka.Routing;
using Serilog;
using Akka.Logger.Serilog;
using Akka.Event;
using Akka.Cluster.Routing;

namespace WatchDog
{
    public class Worker : ReceiveActor
    {
        private readonly string workerId;

        public Worker()
        {
            workerId = Guid.NewGuid().ToString();

            Receive<CalculationJob>(x =>
            {
                int result = 0;
                switch (x.Operation)
                {
                    case "ADD":
                        result = x.Number1 + x.Number2;
                        break;
                    case "SUB":
                        result = x.Number1 - x.Number2;
                        break;
                    case "MULT":
                        result = x.Number1 * x.Number2;
                        break;
                    case "DIV":
                        result = x.Number1 / x.Number2;
                        break;

                }       
                Console.WriteLine("result: " + result.ToString());          
            });
        }
    }

    public class CalculationJob
    {
        public int Number1 { get; private set;}
        public int Number2 { get; private set;}
        public string Operation { get; private set;}
        public CalculationJob(int number1, int number2, string operation)
        {
            Number1 = number1;
            Number2 = number2;
            Operation = operation;
        }
    }
}