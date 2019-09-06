using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JW
{
    public class JWTaskRunner: TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            //若TASK_STEPS_CONCURRENT_LIMIT有調整,該參數也要跟著改
            //int maxConcurentTasks = 5 + 3 + 3;

            //ThreadPool.SetMinThreads(maxConcurentTasks, maxConcurentTasks);
            //ThreadPool.SetMaxThreads(maxConcurentTasks, maxConcurentTasks);

            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                //int workerThreads;
                //int portThreads;
                int toProcess = 0;

                foreach (MyTask task in tasks)
                {
                    Interlocked.Increment(ref toProcess);

                    //ThreadPool.GetMinThreads(out workerThreads, out portThreads);

                    //Console.WriteLine("\nMaximum worker threads: \t{0}" +
                    //    "\nMaximum completion port threads: {1}",
                    //    workerThreads, portThreads);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(DoSomething), task);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                    {
                        //Console.WriteLine(((MyTask)x).ID + " start....1");
                        ((MyTask)x).DoStepN(1);

                        //Console.WriteLine(((MyTask)x).ID + " start....2");
                        ((MyTask)x).DoStepN(2);

                        //Console.WriteLine(((MyTask)x).ID + " start....3");
                        ((MyTask)x).DoStepN(3);

                        if (Interlocked.Decrement(ref toProcess) == 0)
                            resetEvent.Set();
                    }
                ), task);
                }

                resetEvent.WaitOne();
            }
        }
    }

}
