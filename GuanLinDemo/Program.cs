using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuanLinDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var guanlinTaskRunner = new GuanlinTaskRunner1();
            guanlinTaskRunner.ExecuteTasks(1000);
        }
    }

    public class GuanlinTaskRunner1 : TaskRunnerBase
    {
        private readonly int[] _maxConcurrency;
        private readonly List<SemaphoreSlim> _semaphoreSlimList;


        public GuanlinTaskRunner1()
        {
            _maxConcurrency = new[] { 0, 5, 3, 3 };
            _semaphoreSlimList = new List<SemaphoreSlim>{
                null,
                new SemaphoreSlim(_maxConcurrency[1]),
                new SemaphoreSlim(_maxConcurrency[2]),
                new SemaphoreSlim(_maxConcurrency[3])
            };
        }

        public override void Run(IEnumerable<MyTask> tasks)
        {
            Task.WaitAll(tasks.Select(task => Task.Factory.StartNew(() => { GetStepAndRunTask(task); })).ToArray());
        }

        private  void GetStepAndRunTask(MyTask task)
        {
            for (var i = 1; i <= 3; i++)
            {
                try
                {
                    _semaphoreSlimList[i].Wait();
                    task.DoStepN(i);
                }
                finally
                {
                    _semaphoreSlimList[i].Release();
                }
            }
        }
    }
}