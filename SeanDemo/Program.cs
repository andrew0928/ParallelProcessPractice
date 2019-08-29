using System;
using ParallelProcessPractice.Core;

namespace SeanDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase runner = new SeanRunner();
            runner.ExecuteTasks(30);
            Console.ReadKey();
        }
    }
}
