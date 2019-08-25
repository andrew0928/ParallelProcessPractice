using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LexDemo
{
    public class LexTaskRunner : TaskRunnerBase
    {
        private static readonly int _maxCount = Environment.ProcessorCount;
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(_maxCount);

        public override void Run(IEnumerable<MyTask> tasks)
        {
            var processTasks = tasks.Select(processTask)
                                    .ToArray();
            Task.WaitAll(processTasks);
        }

        private Task processTask(MyTask myTask)
        {
            _semaphoreSlim.Wait();
            return Task.Run(async () =>
            {
                await Task.Run(() => myTask.DoStepN(1)).ConfigureAwait(false);
                await Task.Run(() => myTask.DoStepN(2)).ConfigureAwait(false);
                await Task.Run(() => myTask.DoStepN(3)).ConfigureAwait(false);
                _semaphoreSlim.Release();
            });
        }
    }
}
