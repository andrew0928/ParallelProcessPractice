using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace JolinDemo
{
    public class TaskPipeline
    {
        private Queue<MyTask> _queue;
        private ManualResetEvent _mre;
        private List<Thread> _threadPool;

        private bool _finished;
        public int _step;
        public TaskPipeline NextPipeline { get; set; }

        public TaskPipeline(int step, int concurentlimit)
        {
            _finished = false;
            _queue = new Queue<MyTask>();
            _mre = new ManualResetEvent(false);
            _step = step;
            _threadPool = new List<Thread>();

            for (int i = 0; i < concurentlimit; i++)
            {
                Thread thread = new Thread(ThreadBody);
                thread.Start();
                _threadPool.Add(thread);
            }
        }

        public void AddTask(MyTask task)
        {
            lock (this._queue)
            {
                this._queue.Enqueue(task);

                Console.WriteLine($"pipeline_{_step} 收到 Task: {task.ID}");

            }
            this._mre.Set();
            this._mre.Reset();
        }

        private void ThreadBody()
        {
            while (true)
            {
                while (_queue.Count > 0)
                {
                    MyTask task = null;
                    lock (_queue)
                    {
                        _queue.TryDequeue(out task);
                    }

                    if (task != null)
                    {
                        task.DoStepN(_step);
                        Console.WriteLine($"pipeline_{_step} 執行完 Task: {task.ID}");
                        if (NextPipeline != null)
                        {
                            NextPipeline.AddTask(task);
                        }
                    }
                }

                if (this._finished)
                {
                    Console.WriteLine($"Thread_{Thread.CurrentThread.ManagedThreadId} close");
                    break;
                }
                else
                {
                    _mre.WaitOne();
                }
            }
        }

        public async Task WaitFinished()
        {
            _finished = true;
            this._mre.Set();

            foreach (var thread in _threadPool) thread.Join();

            Console.WriteLine($"pipeline_{_step} close");
            if (NextPipeline != null)
            {
                await NextPipeline.WaitFinished();
            }
        }
    }
}