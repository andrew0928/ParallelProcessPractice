using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JW
{
    public class JWTaskRunnerV2 : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            int maxConcurentTasks = 11+6;
            ThreadPool.SetMinThreads(maxConcurentTasks, maxConcurentTasks);
            ThreadPool.SetMaxThreads(maxConcurentTasks, maxConcurentTasks);
            int totalToProcess = 0;

            int toProcess = 0;

            int bc2Count = 0;

            int bc3Count = 0;

            BlockingCollection<MyTask> bc1 = new BlockingCollection<MyTask>();

            BlockingCollection<MyTask> bc2 = new BlockingCollection<MyTask>();

            BlockingCollection<MyTask> bc3 = new BlockingCollection<MyTask>();

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
                      while (!bc1.IsCompleted)
                      {
                          MyTask task = bc1.Take();

                          Task.Run(async () =>
                          {
                              await Task.Factory.StartNew(() =>
                              {
                                  task.DoStepN(1);
                              });
                              

                              Console.WriteLine(task.ID + " dostep 1 "+DateTime.Now+"...finish");
                              bc2.Add(task);
                              Interlocked.Increment(ref bc2Count);
                          });

                          //Task.Factory.StartNew(() =>
                          //{
                          //    task.DoStepN(1);

                          //    Console.WriteLine(task.ID + " dostep 1...finish");
                          //    bc2.Add(task);
                          //    Interlocked.Increment(ref bc2Count);
                          //});

                          //ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                          //{
                          //    ((MyTask)x).DoStepN(1);

                          //    Console.WriteLine(((MyTask)x).ID + " dostep 1...finish");
                          //    bc2.Add(((MyTask)x));
                          //    Interlocked.Increment(ref bc2Count);
                          //}), task);
                      }
                      Console.WriteLine("bc1 complete!!!!!!");
                  });


                Task bc2Runner = Task.Run( () =>
                  {
                      while (!bc2.IsCompleted)
                      {
                          if (bc2Count == totalToProcess)
                              bc2.CompleteAdding();

                          Console.WriteLine(bc2.Count);

                          MyTask task = bc2.Take();

                           Task.Run(async () =>
                          {
                              await Task.Factory.StartNew(() =>
                              {
                                  task.DoStepN(2);
                              });

                              Console.WriteLine("do " + task.ID + " dostep 2...");
                              bc3.Add(task);
                              Interlocked.Increment(ref bc3Count);
                          });

                          //Task.Factory.StartNew(() =>
                          //{
                          //    Console.WriteLine("do " + task.ID + " dostep 2...");
                          //    task.DoStepN(2);

                          //    bc3.Add(task);
                          //    Interlocked.Increment(ref bc3Count);
                          //});

                          //ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                          //{
                          //    Console.WriteLine("do " + ((MyTask)x).ID + " dostep 2...");
                          //    ((MyTask)x).DoStepN(2);

                          //    bc3.Add(((MyTask)x));
                          //    Interlocked.Increment(ref bc3Count);
                          //}), task);
                      }
                      Console.WriteLine("bc2 complete!!!!!!");
                  });


                Task task3Runner = Task.Run(() =>
                {
                    while (!bc3.IsCompleted)
                    {
                        if (bc3Count == totalToProcess)
                            bc3.CompleteAdding();

                        MyTask task = bc3.Take();

                        Task.Run(async() =>
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                task.DoStepN(3);
                            });
                            
                            Console.WriteLine(task.ID + " dostep 3...");

                            if (Interlocked.Decrement(ref toProcess) == 0)
                                resetEvent.Set();
                        });
                        //Task.Factory.StartNew(() =>
                        //{
                        //    task.DoStepN(3);
                        //    Console.WriteLine(task.ID + " dostep 3...");

                        //    if (Interlocked.Decrement(ref toProcess) == 0)
                        //        resetEvent.Set();
                        //});
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                        //{
                        //    ((MyTask)x).DoStepN(3);
                        //    Console.WriteLine(((MyTask)x).ID + " dostep 3...");

                        //    if (Interlocked.Decrement(ref toProcess) == 0)
                        //        resetEvent.Set();
                        //}), task);
                    }
                    Console.WriteLine("bc3 complete!!!!!!");
                });


                resetEvent.WaitOne();
            }
        }
    }

}
