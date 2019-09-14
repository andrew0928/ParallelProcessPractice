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
                run.ExecuteTasks(1000);
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
                // catalog: threads / async tasks
                yield return new LexDemo.LexTaskRunner();

                // catalog: pipeline + async tasks
                yield return new SeanDemo.SeanRunner();

                // catalog: threads / async tasks
                yield return new EPDemo.EPTaskRunner();

                // catalog: pipeline + rtx
                yield return new PhoenixDemo.PhoenixTaskRunner();

                // catalog: threads / async tasks
                yield return new JulianDemo.TaskRunner();

                // catalog: pipeline + async tasks
                yield return new GuluDemo.GuluTaskRunner();

                // catalog: pipeline + share threads (thread pool)
                yield return new JW.JWTaskRunnerV5();

                // catalog: threads / async tasks / TPL
                yield return new AndyDemo.AndyTaskRunner();





                // demo: min wip
                yield return new AndrewDemo.AndrewBasicTaskRunner1();

                // demo: min context switch (between steps)
                yield return new AndrewDemo.AndrewBasicTaskRunner2();

                // demo: parallel process only, using TPL / PLinq
                yield return new AndrewDemo.AndrewThreadTaskRunner1();

                // demo: pipeline flow control, using thread(s)
                yield return new AndrewDemo.AndrewPipelineTaskRunner1();

                // demo: pipeline flow control, using threads (between steps) + PLinq (parallelism in step)
                yield return new AndrewDemo.AndrewPipelineTaskRunner2();

                yield return new BorisDemo.BorisTaskRunner(5,3,3);
                yield return new LeviDemo.LeviTaskRunner();
                yield return new JolinDemo.JolinTaskRunner(3, 5, 3, 3, 3);
                yield return new JW.JWTaskRunnerV3();
            }
        }
    }
}
