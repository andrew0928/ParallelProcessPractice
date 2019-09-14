using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AndrewDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run =
                //new AndrewBasicTaskRunner1();
                //new AndrewBasicTaskRunner2();
                //new AndrewThreadTaskRunner1();
                new AndrewPipelineTaskRunner1();
                //new AndrewPipelineTaskRunner2();
            run.ExecuteTasks(1000);
        }

    }
}
