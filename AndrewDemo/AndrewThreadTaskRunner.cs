using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

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
                .WithDegreeOfParallelism(11)
                .ForAll((task) =>
                {
                    task.DoStepN(1);
                    task.DoStepN(2);
                    task.DoStepN(3);
                });
        }
    }

    public class AndrewThreadTaskRunner2 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            tasks.AsParallel()
                .WithDegreeOfParallelism(11)
                .ForAll((t) =>
                {
                    Task.Run(() => { t.DoStepN(1); })
                    .ContinueWith((x) => { t.DoStepN(2); })
                    .ContinueWith((x) => { t.DoStepN(3); })
                    .Wait();
                });
        }
    }
}
