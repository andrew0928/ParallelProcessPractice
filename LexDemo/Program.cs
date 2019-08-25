using ParallelProcessPractice.Core;
using System;

namespace LexDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase taskRunner = new LexTaskRunner();
            taskRunner.ExecuteTasks(1000);

            Console.ReadKey();
        }
    }
}
