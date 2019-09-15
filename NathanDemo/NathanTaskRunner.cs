using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NathanDemo
{
    public class NathanTaskRunner : TaskRunnerBase
    {
        private readonly ConcurrentExclusiveSchedulerPair step1 =
                new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, maxConcurrencyLevel: 5);

        private readonly ConcurrentExclusiveSchedulerPair step2 =
        new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, maxConcurrencyLevel: 3);

        private readonly ConcurrentExclusiveSchedulerPair step3 =
    new ConcurrentExclusiveSchedulerPair(TaskScheduler.Default, maxConcurrencyLevel: 3);

        public override void Run(IEnumerable<MyTask> tasks)
        {
            var processTasks = tasks.Select(processTask).ToArray();
            Task.WaitAll(processTasks);
        }

        private Task processTask(MyTask myTask)
        {
            CancellationToken cts = new CancellationToken();
            return Task.Run(async () =>
              {
                  await Task.Factory.StartNew(() => myTask.DoStepN(1), cts,
                             TaskCreationOptions.None, step1.ConcurrentScheduler)
                          .ContinueWith(x => myTask.DoStepN(2), step2.ConcurrentScheduler)
                          .ContinueWith(x => myTask.DoStepN(3), step3.ConcurrentScheduler)
                          ;
              });
        }
    }
}