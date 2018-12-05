using System;

namespace Shared
{
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

    public class CalculationResult
    {
        public CalculationResult(int result)
        {
            Result = result;
        }

        public int Result { get; }
    }

    public class UnknownOperationException : Exception
    {
    }
}