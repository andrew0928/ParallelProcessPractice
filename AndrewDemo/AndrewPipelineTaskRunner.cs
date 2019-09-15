using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            bool _first = (step == 1);
            bool _last = (step == 3);

            //foreach (var task in this.queues[step].GetConsumingEnumerable())
            //{
            //    task.DoStepN(step);
            //    if (!_last) this.queues[step + 1].Add(task);
            //}

            //System.Threading.Tasks.Parallel.ForEach(this.queues[step].GetConsumingEnumerable(), (task) =>
            //{
            //    task.DoStepN(step);
            //    if (!_last) this.queues[step + 1].Add(task);
            //});

            this.queues[step]
                .GetConsumingEnumerable()
                .AsParallel()
                //.WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                //.WithMergeOptions(ParallelMergeOptions.)
                .WithDegreeOfParallelism(5)
                .ForAll((task) =>
                {
                    task.DoStepN(step);
                    if (!_last) this.queues[step + 1].Add(task);
                });

            if (!_last) this.queues[step + 1].CompleteAdding();
        }
    }

}
