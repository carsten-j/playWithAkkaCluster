using Akka.Actor;
using System;

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
        public CalculationJob(int number1, int number2, string operation)
        {
            Number1 = number1;
            Number2 = number2;
            Operation = operation;
        }

        public int Number1 { get; }
        public int Number2 { get; }
        public string Operation { get; }
    }
}