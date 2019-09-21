using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AndrewDemo
{
    // pipeline control



    // use threads + blocking collection
    public class AndrewPipelineTaskRunner1 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            List<Thread> threads = new List<Thread>();

            Thread t = null;
            int[] counts = { 0,
                5,
                3,
                3
            };

            for (int step = 1; step <= 3; step++)
            {
                for (int i = 0; i < counts[step]; i++)
                {
                    threads.Add(t = new Thread(this.DoAllStepN)); t.Start(step);
                }
            }


            foreach (var task in tasks) this.queues[1].Add(task);

            for (int step = 1; step <= 3; step++)
            {
                this.queues[step].CompleteAdding();
                for (int i = 0; i < counts[step]; i++)
                {
                    threads[0].Join();
                    threads.RemoveAt(0);
                }
            }
        }

        private BlockingCollection<MyTask>[] queues = new BlockingCollection<MyTask>[3 + 1]
        {
            null,
            new BlockingCollection<MyTask>(),
            new BlockingCollection<MyTask>(),
            new BlockingCollection<MyTask>(),
        };

        private void DoAllStepN(object step_value)
        {
            int step = (int)step_value;
            bool _first = (step == 1);
            bool _last = (step == 3);

            foreach (var task in this.queues[step].GetConsumingEnumerable())
            {
                task.DoStepN(step);
                if (!_last) this.queues[step + 1].Add(task);
            }
        }
    }



    // use plinq + blocking collection
    public class AndrewPipelineTaskRunner2 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            List<Thread> threads = new List<Thread>();

            for (int step = 1; step <= 3; step++)
            {
                var t = new Thread(this.DoAllStepN);
                t.Start(step);
                threads.Add(t);
            }

            foreach (var task in tasks) this.queues[1].Add(task);
            queues[1].CompleteAdding();


            foreach (var t in threads) t.Join();
        }

        private BlockingCollection<MyTask>[] queues = new BlockingCollection<MyTask>[3 + 1]
        {
            null,
            new BlockingCollection<MyTask>(),
            new BlockingCollection<MyTask>(),
            new BlockingCollection<MyTask>(),
        };

        private void DoAllStepN(object step_value)
        {
            int step = (int)step_value;
            bool _is_first_step = (step == 1);
            bool _is_last_step = (step == 3);

            this.queues[step]
                .GetConsumingEnumerable()
                .AsParallel()
                //.WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                //.WithMergeOptions(ParallelMergeOptions.)
                .WithDegreeOfParallelism(5)
                .ForAll((task) =>
                {
                    task.DoStepN(step);
                    if (!_is_last_step) this.queues[step + 1].Add(task);
                });

            if (!_is_last_step) this.queues[step + 1].CompleteAdding();
        }
    }


    public class AndrewPipelineTaskRunner3 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            Task t1 = this.DoAllStepNAsync(1);
            Task t2 = this.DoAllStepNAsync(2);
            Task t3 = this.DoAllStepNAsync(3);

            Task.WaitAll(tasks.Select(t => Task.Run(async () =>
            {
                await this.queues[1].Writer.WriteAsync(t);
            })).ToArray());

            queues[1].Writer.Complete();

            Task.WaitAll(t1, t2, t3);
        }

        private Channel<MyTask>[] queues = new Channel<MyTask>[3 + 1]
        {
            null,
            Channel.CreateBounded<MyTask>(5),
            Channel.CreateUnbounded<MyTask>(),
            Channel.CreateUnbounded<MyTask>(),
        };

        private async Task DoAllStepNAsync(int step) {
            bool last = (step == 3);
            List<Task> ts = new List<Task>();
            while (await this.queues[step].Reader.WaitToReadAsync())
            {
                while (this.queues[step].Reader.TryRead(out MyTask task))
                {
                    ts.Add(Task.Run(async () =>
                    {
                        task.DoStepN(step);
                        if (!last) await this.queues[step + 1].Writer.WriteAsync(task);
                    }));
                }
            }
            Task.WaitAll(ts.ToArray());
            if (!last) this.queues[step + 1].Writer.Complete();
        }
    }
}
