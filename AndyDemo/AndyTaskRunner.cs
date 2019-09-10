using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace AndyDemo
{
    public class AndyTaskRunner : TaskRunnerBase
    {
        public SemaphoreSlim sem_1 = new SemaphoreSlim(5);
        public SemaphoreSlim sem_2 = new SemaphoreSlim(3);
        public SemaphoreSlim sem_3 = new SemaphoreSlim(3);

        public override void Run(IEnumerable<MyTask> tasks)
        {
            Parallel.ForEach(tasks, (myTask)=>{
                sem_1.Wait();
                myTask.DoStepN(1);
                sem_1.Release();

                sem_2.Wait();
                myTask.DoStepN(2);
                sem_2.Release();

                sem_3.Wait();
                myTask.DoStepN(3);
                sem_3.Release();
            });
        }
    }
}
