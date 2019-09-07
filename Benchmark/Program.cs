using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var run in Runners)
            {
                Console.Error.WriteLine($"==========================================================");
                Console.Error.WriteLine($"Runner: {run.GetType().FullName}");
                Console.Error.WriteLine($"==========================================================");
                run.ExecuteTasks(30);
                Console.Error.WriteLine();
                Console.Error.WriteLine();
            }
        }


        static IEnumerable<TaskRunnerBase> Runners
        {
            get
            {
                yield return new JW.JWTaskRunnerV5();
            }
        }
    }
}
