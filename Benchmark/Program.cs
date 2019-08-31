using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using JW;

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
                yield return new AndrewDemo.AndrewTaskRunner();
                yield return new LexDemo.LexTaskRunner();
                yield return new JulianDemo.TaskRunner();
                yield return new JWTaskRunner();
            }
        }
    }
}
