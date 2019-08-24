using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewDemo
{
    public class AndrewTaskRunner : TaskRunnerBase
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
}
