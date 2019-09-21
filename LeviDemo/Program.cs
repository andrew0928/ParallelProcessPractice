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
            
            run.ExecuteTasks(1000); 
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
