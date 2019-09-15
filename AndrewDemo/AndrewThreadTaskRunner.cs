using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AndrewDemo
{

    //
    //  just use multiple threads, no sync control
    //


    // use plinq
    public class AndrewThreadTaskRunner1 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            tasks.AsParallel()
                .WithDegreeOfParallelism(10)
                .ForAll((task) =>
                {
                    task.DoStepN(1);
                    task.DoStepN(2);
                    task.DoStepN(3);
                });
        }
    }
}
