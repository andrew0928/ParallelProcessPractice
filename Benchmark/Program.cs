using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var run in Runners)
            {
                Console.Error.WriteLine($"==========================================================");
                Console.Error.WriteLine($"Runner: {run.GetType().FullName}");
                Console.Error.WriteLine($"==========================================================");
                run.ExecuteTasks(1000);
                Console.Error.WriteLine();
                Console.Error.WriteLine();
            }

            //Console.WriteLine("Press [ENTER] to exit...");
            //Console.ReadKey();
        }

        static IEnumerable<TaskRunnerBase> Runners
        {
            get
            {
                yield return new NathanDemo.NathanTaskRunner();
                yield return new LexDemo.LexTaskRunner();
                yield return new SeanDemo.SeanRunner();
                yield return new EPDemo.EPTaskRunner();
                yield return new PhoenixDemo.PhoenixTaskRunner();
                yield return new JulianDemo.TaskRunner();
                yield return new GuluDemo.GuluTaskRunner();
                yield return new JW.JWTaskRunnerV5();
                yield return new AndyDemo.AndyTaskRunner();
                yield return new MazeDemo.MazeTaskRunner();

                yield return new AndrewDemo.AndrewBasicTaskRunner1();
                yield return new AndrewDemo.AndrewBasicTaskRunner2();
                yield return new AndrewDemo.AndrewThreadTaskRunner1();
                yield return new AndrewDemo.AndrewPipelineTaskRunner1();
                yield return new AndrewDemo.AndrewPipelineTaskRunner2();

                yield return new BorisDemo.BorisTaskRunner(5, 3, 3);
                yield return new JolinDemo.JolinTaskRunner(3, 5, 3, 3);
                yield return new LeviDemo.LeviTaskRunner();


            }
        }
    }
}