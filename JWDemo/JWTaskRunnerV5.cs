using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JW
{
    public class JWTaskRunnerV5 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            int maxConcurentTasks = (5 + 3 + 3) + 3;
            ThreadPool.SetMinThreads(maxConcurentTasks, maxConcurentTasks);
            ThreadPool.SetMaxThreads(maxConcurentTasks, maxConcurentTasks);

            int toProcess = 0;

            Queue.Init();

            //initial Step 1 in queue
            foreach (MyTask task in tasks)
            {
                Queue.Produce(1, task);

                Interlocked.Increment(ref toProcess);
            }

            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                Task.Run(() =>
                {
                    JobWorker jobWorker1 = new JobWorker(1, 5);
                });


                Task.Run(() =>
                {
                    JobWorker jobWorker2 = new JobWorker(2, 3);
                });


                Task.Run(() =>
                {
                    JobWorker jobWorker3 = new JobWorker(3, 3, toProcess, resetEvent);
                });

                resetEvent.WaitOne();
            }
        }

        internal class Queue
        {
            private static ConcurrentDictionary<int, BlockingCollection<MyTask>> queue = new ConcurrentDictionary<int, BlockingCollection<MyTask>>();

            public static void Init()
            {
                queue.TryAdd(1, new BlockingCollection<MyTask>());
                queue.TryAdd(2, new BlockingCollection<MyTask>());
                queue.TryAdd(3, new BlockingCollection<MyTask>());
            }

            public static void Produce(int step, MyTask task)
            {
                queue.GetOrAdd(step, new BlockingCollection<MyTask>()).Add(task);
            }

            public static MyTask Poll(int step)
            {
                BlockingCollection<MyTask> collection;

                queue.TryGetValue(step, out collection);

                return collection.Take();
            }

        }

        internal class JobWorker
        {
            private Semaphore semaphore;

            public JobWorker(int whichStep, int threadLimit)
            {
                semaphore = new Semaphore(threadLimit, threadLimit);

                while (true)
                {
                    MyTask task = Queue.Poll(whichStep);

                    semaphore.WaitOne();

                    Task.Run(() =>
                    {
                        task.DoStepN(whichStep);

                        Queue.Produce(task.CurrentStep + 1, task);

                        semaphore.Release();
                    });
                }
            }

            public JobWorker(int whichStep, int threadLimit, int toProcess, ManualResetEvent resetEvent)
            {
                semaphore = new Semaphore(threadLimit, threadLimit);

                while (true)
                {
                    MyTask task = Queue.Poll(whichStep);

                    semaphore.WaitOne();

                    Task.Run(() =>
                    {
                        task.DoStepN(whichStep);

                        semaphore.Release();

                        if (Interlocked.Decrement(ref toProcess) == 0)
                            resetEvent.Set();
                    });
                }
            }
        }
    }
}
