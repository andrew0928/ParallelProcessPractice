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

            Console.WriteLine("Press [ENTER] to exit...");
            Console.ReadKey();
        }


        static IEnumerable<TaskRunnerBase> Runners
        {
            get
            {
                yield return new AndrewDemo.AndrewTaskRunner();
                yield return new LexDemo.LexTaskRunner();
                yield return new SeanDemo.SeanRunner();
                yield return new EPDemo.EPTaskRunner();
                yield return new PhoenixDemo.PhoenixTaskRunner();
                yield return new JulianDemo.TaskRunner();
                yield return new GuluDemo.GuluTaskRunner();
                yield return new JW.JWTaskRunnerV5();
            }
        }
    }
}
