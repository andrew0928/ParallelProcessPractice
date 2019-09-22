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
                GC.Collect();
                run.ExecuteTasks(1000);
                Console.Error.WriteLine();
                Console.Error.WriteLine();
            }
        }

        static IEnumerable<TaskRunnerBase> Runners
        {
            get
            {
                // contributors from facebook

                // multitask
                yield return new LexDemo.LexTaskRunner();           // PR#1

                // pipeline
                yield return new EPDemo.EPTaskRunner();             // PR#3

                // pipeline
                yield return new SeanDemo.SeanRunner();             // PR#4

                // pipeline
                yield return new PhoenixDemo.PhoenixTaskRunner();   // PR#5

                // pipeline
                yield return new JulianDemo.TaskRunner();           // PR#6

                // pipeline
                yield return new GuluDemo.GuluTaskRunner();         // PR#7

                // pipeline
                yield return new JW.JWTaskRunnerV5();               // PR#8, PR#2

                // multitask
                yield return new AndyDemo.AndyTaskRunner();         // PR#10

                // multitask
                yield return new MazeDemo.MazeTaskRunner();         // PR#11, PR#9

                // pipeline
                yield return new NathanDemo.NathanTaskRunner();     // PR#12

                // team members from 91APP

                // pipeline
                yield return new BorisDemo.BorisTaskRunner(5, 3, 3);

                // pipeline
                yield return new JolinDemo.JolinTaskRunner(3, 5, 3, 3);

                // multitask
                yield return new LeviDemo.LeviTaskRunner();

                // my demo codes

                // other
                yield return new AndrewDemo.AndrewBasicTaskRunner1();
                yield return new AndrewDemo.AndrewBasicTaskRunner2();

                // multitask
                yield return new AndrewDemo.AndrewThreadTaskRunner1();
                yield return new AndrewDemo.AndrewThreadTaskRunner2();

                // pipeline
                yield return new AndrewDemo.AndrewPipelineTaskRunner1();
                yield return new AndrewDemo.AndrewPipelineTaskRunner2();
                yield return new AndrewDemo.AndrewPipelineTaskRunner3();
            }
        }
    }
}