using ParallelProcessPractice.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AnthonyDemo
{
    internal class AnthonyTaskRunner : TaskRunnerBase
    {
        private IWorker worker1;
        private IWorker worker2;
        private IWorker worker3;

        private void InitWorker(IEnumerable<MyTask> tasks)
        {
            int taskCount = tasks.Count();

            worker1 = new Worker(1, new ConcurrentQueue<MyTask>(tasks), taskCount);
            worker2 = new Worker(2, new ConcurrentQueue<MyTask>(), taskCount);
            worker3 = new Worker(3, new ConcurrentQueue<MyTask>(), taskCount);

            worker1.Next = worker2;
            worker2.Next = worker3;

            worker1.Receive(5);
            worker2.Receive(3);
            worker3.Receive(3);
        }

        private void WaitAllTaskFinished(int count)
        {
            while (worker1.IsFinished == false ||
                   worker2.IsFinished == false ||
                   worker3.IsFinished == false)
            {
                ;
            }
        }

        public override void Run(IEnumerable<MyTask> tasks)
        {
            InitWorker(tasks);
            WaitAllTaskFinished(tasks.Count());
        }
    }
}