using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewDemo
{
    //
    //  just use one thread, no parallel process
    //


    //
    //  min wip
    //
    public class AndrewBasicTaskRunner1 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            foreach (var task in tasks)
            {
                task.DoStepN(1);
                task.DoStepN(2);
                task.DoStepN(3);
            }
        }
    }


    //
    //  min context switch between step(s)
    //
    public class AndrewBasicTaskRunner2 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            var tasklist = tasks.ToArray();

            foreach (var task in tasklist) task.DoStepN(1);
            foreach (var task in tasklist) task.DoStepN(2);
            foreach (var task in tasklist) task.DoStepN(3);
        }
    }
}
