using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JW
{
    public class JWTaskRunnerV4 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            int maxConcurentTasks = 11 + 3;
            ThreadPool.SetMinThreads(maxConcurentTasks, maxConcurentTasks);
            ThreadPool.SetMaxThreads(maxConcurentTasks, maxConcurentTasks);
            int totalToProcess = 0;

            int toProcess = 0;

            int bc2Count = 0;

            int bc3Count = 0;

            BlockingCollection<MyTask> bc1 = new BlockingCollection<MyTask>();

            BlockingCollection<MyTask> bc2 = new BlockingCollection<MyTask>();

            BlockingCollection<MyTask> bc3 = new BlockingCollection<MyTask>();

            Semaphore semaphore1 = new Semaphore(5, 5);
            Semaphore semaphore2 = new Semaphore(3, 3);
            Semaphore semaphore3 = new Semaphore(3, 3);

            foreach (MyTask task in tasks)
            {
                bc1.Add(task);

                Interlocked.Increment(ref toProcess);
            }
            totalToProcess = toProcess;

            bc1.CompleteAdding();

            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                Task bc1Runner = Task.Run(() =>
                  {
                      while (true)
                      {
                          MyTask task = bc1.Take();
                          semaphore1.WaitOne();
                          Task.Run(() =>
                          {
                              {
                                  task.DoStepN(1);
                                  Console.WriteLine("do " + task.ID + " dostep 1...END");
                                  bc2.Add(task);
                                  Interlocked.Increment(ref bc2Count);
                              }
                              semaphore1.Release();
                          });
                      }
                      Console.WriteLine("bc1 complete!!!!!!");
                  });


                Task bc2Runner = Task.Run(() =>
                 {
                     while (true)
                     {
                         if (bc2Count == totalToProcess)
                             bc2.CompleteAdding();

                         Console.WriteLine(bc2.Count);

                         MyTask task = bc2.Take();

                         semaphore2.WaitOne();
                         Task.Run(() =>
                         {
                             {
                                 task.DoStepN(2);
                                 Console.WriteLine("do " + task.ID + " dostep 2...END");
                                 bc3.Add(task);
                                 Interlocked.Increment(ref bc3Count);
                             }
                             semaphore2.Release();
                         });
                     }
                     Console.WriteLine("bc2 complete!!!!!!");
                 });


                Task task3Runner = Task.Run(() =>
                {
                    while (true)
                    {
                        if (bc3Count == totalToProcess)
                            bc3.CompleteAdding();

                        MyTask task = bc3.Take();

                        semaphore3.WaitOne();
                        Task.Run(() =>
                        {
                            {
                                task.DoStepN(3);
                                Console.WriteLine(task.ID + " dostep 3...");

                                if (Interlocked.Decrement(ref toProcess) == 0)
                                    resetEvent.Set();
                            }
                            semaphore3.Release();
                        });

                    }
                    Console.WriteLine("bc3 complete!!!!!!");
                });


                resetEvent.WaitOne();
            }
        }

        internal class Queue {
            private static Dictionary<int, BlockingCollection<MyTask>> queue = new Dictionary<int, BlockingCollection<MyTask>>();

            public static void Produce(int step, MyTask task) {
                BlockingCollection<MyTask> collection;

                if (queue.TryGetValue(step, out collection))
                {
                    collection.Add(task);
                }
                else {
                    queue.Add(step, new BlockingCollection<MyTask>());
                }
            }

            public static MyTask Poll(int step) {
                BlockingCollection<MyTask> collection;

                queue.TryGetValue(step, out collection);

                return collection.Take();
            }

        }

        internal class JobWorker{

            public JobWorker(int WhichStep) {
            }
        }


    }

}
