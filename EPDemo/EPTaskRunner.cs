using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPDemo
{
    public class EPTaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            // Parallel.ForEach(tasks, (task) =>
            // {
            //     task.DoStepN(1);
            //     task.DoStepN(2);
            //     task.DoStepN(3);
            // });
            foreach (var oo in RunStepNAsync(RunStepNAsync(RunStepNAsync(tasks, 1), 2), 3));
        }        

        public static IEnumerable<MyTask> RunStepNAsync(IEnumerable<MyTask> tasks, int n)
        {
            Task<MyTask> workingTask = null;
            foreach (var task in tasks)
            {
                if (workingTask != null)
                {
                    yield return workingTask.GetAwaiter().GetResult();
                }

                workingTask = Task.Run<MyTask>(() =>
                {
                    task.DoStepN(n);
                    return task;
                });
            }

            if (workingTask != null)
            {
                yield return workingTask.GetAwaiter().GetResult();
            }
        }
    }
}
