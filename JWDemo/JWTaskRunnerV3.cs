using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JW
{

    /**
     * Another way for multi threading!!!
     */
    public class JWTaskRunnerV3 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {

            int toProcess = 0;

            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                foreach (MyTask task in tasks)
                {
                    Interlocked.Increment(ref toProcess);

                    Task.Run(() =>
                     {
                         task.DoStepN(1);
                     }).ContinueWith(tk =>
                     {
                         task.DoStepN(2);
                     }).ContinueWith(tk =>
                     {
                         task.DoStepN(3);

                         if (Interlocked.Decrement(ref toProcess) == 0)
                             resetEvent.Set();
                     });
                }

                resetEvent.WaitOne();
            }
        }
    }

}
