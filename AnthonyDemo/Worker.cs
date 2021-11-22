using ParallelProcessPractice.Core;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AnthonyDemo
{
    public class Worker : IWorker
    {
        public IWorker Next { get; set; }

        public ConcurrentQueue<MyTask> Queue { get; set; }

        public int Step { get; set; }

        public int ExpectCount { get; set; }

        public int FinishedCount { get; set; }

        public bool IsFinished { get => ExpectCount == FinishedCount; }

        private object _lock = new object();

        public Worker(int step, ConcurrentQueue<MyTask> queue, int expectCount)
        {
            this.Queue = queue;
            this.Step = step;
            this.ExpectCount = expectCount;
        }

        public void Receive(int startCount)
        {
            for (int index = 1; index <= startCount; index++)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (Queue.TryDequeue(out MyTask item))
                        {
                            item.DoStepN(Step);

                            lock (_lock)
                            {
                                FinishedCount++;
                            }

                            if (Next != null)
                            {
                                Next.Queue.Enqueue(item);
                            }
                        }
                    }
                });
            }
        }
    }
}