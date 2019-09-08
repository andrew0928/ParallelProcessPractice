using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LeviDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new LeviTaskRunner();
            
            // 60,965.1485 msec vs 30,466.7193 msec
            // Execution Summary:
            //     - Result: PASS
            //     - Total / Success / Fail Count: 100 / 100 / 0
            //     - Execute Time: 60965.1485 msec
            // Execution Summary:
            //     - Result: PASS
            //     - Total / Success / Fail Count: 100 / 100 / 0
            //     - Execute Time: 30466.7193 msec
            run.ExecuteTasks(100); 

            // 609047.2131 msec vs 301338.2373 msec
            // Execution Summary:
            //     - Result: PASS
            //     - Total / Success / Fail Count: 1000 / 1000 / 0
            //     - Execute Time: 609047.2131 msec
            // Execution Summary:
            //     - Result: PASS
            //     - Total / Success / Fail Count: 1000 / 1000 / 0
            //     - Execute Time: 301338.2373 msec
            // run.ExecuteTasks(1000); 
        }
    }

    public class LeviTaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            SimpleThreadPool stp = new SimpleThreadPool(9);

            foreach (var t in tasks)
            {
                // Console.WriteLine($"Put job: {t.ID} completed.");
                stp.QueueUserWorkerItem(t);
                Thread.Sleep(new Random().Next(100));
            }

            // System.Console.WriteLine("Waiting stop");
            stp.EndPool();
        }
    }
}
