using ParallelProcessPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GuluDemo
{
    public class GuluTaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            //By Less Memory Usage
            //var worker = new BlockingCollectionWorker(tasks, 3);
            //worker.DoWork();

            var worker = new Worker(tasks, 3);
            worker.DoWork();
        }

        public class BlockingCollectionWorker
        {
            private readonly BlockingCollection<MyTask> _step1Queue;
            private readonly BlockingCollection<MyTask> _step2Queue;
            private readonly BlockingCollection<MyTask> _step3Queue;
            private readonly IEnumerable<MyTask> _myTasks;
            private readonly int _consumerCount;
            private int _step1ExecutingCount=0;
            private int _step2ExecutingCount=0;

            public BlockingCollectionWorker(IEnumerable<MyTask> myTasks,int consumerCount)
            {
                if (consumerCount <= 0) throw new ArgumentOutOfRangeException();
                _consumerCount = consumerCount;
                _step1Queue = new BlockingCollection<MyTask>(consumerCount);
                _step2Queue = new BlockingCollection<MyTask>();
                _step3Queue = new BlockingCollection<MyTask>();
                _myTasks = myTasks;
            }

            public void DoWork()
            {
                var taskList=new List<Action>();
                taskList.Add(ProducerRun);
                for (int i = 0; i < _consumerCount; i++)
                {
                    taskList.Add(Setp1ConsumerRun);
                    taskList.Add(Step2ConsumerRun);
                    taskList.Add(Step3ConsumerRun);
                }
                Parallel.Invoke(taskList.ToArray());
            }

            private void ProducerRun()
            {
                foreach (var myTask in _myTasks)
                {
                    _step1Queue.Add(myTask);
                }
                _step1Queue.CompleteAdding();
            }

            private void Setp1ConsumerRun()
            {
                foreach (var myTask in _step1Queue.GetConsumingEnumerable())
                {
                    Interlocked.Increment(ref _step1ExecutingCount);
                    myTask.DoStepN(1);
                    Interlocked.Decrement(ref _step1ExecutingCount);
                    _step2Queue.Add(myTask);
                }
                if(_step1ExecutingCount==0) _step2Queue.CompleteAdding();
            }

            private void Step2ConsumerRun()
            {
                foreach (var myTask in _step2Queue.GetConsumingEnumerable())
                {
                    Interlocked.Increment(ref _step2ExecutingCount);
                    myTask.DoStepN(2);
                    Interlocked.Decrement(ref _step2ExecutingCount);
                    _step3Queue.Add(myTask);
                }
                if (_step2ExecutingCount == 0) _step3Queue.CompleteAdding();
            }

            private void Step3ConsumerRun()
            {
                foreach (var myTask in _step3Queue.GetConsumingEnumerable())
                {
                    myTask.DoStepN(3);
                }
            }
        }

        public class Worker
        {
            private readonly ConcurrentQueue<MyTask> _workQueue;
            private readonly IEnumerable<MyTask> _myTasks;
            private readonly int _threadCount;
            private bool _isComplete = false;

            public Worker(IEnumerable<MyTask> myTasks, int threadCount)
            {
                if (threadCount <= 0) throw new ArgumentOutOfRangeException();
                _workQueue = new ConcurrentQueue<MyTask>();
                _myTasks = myTasks;
                _threadCount = threadCount;
            }

            public void DoWork()
            {
                var tasks = new Task[_threadCount];
                for (var i = 0; i < _threadCount; i++)
                {
                    tasks[i] = Task.Factory.StartNew(ConsumerRun, TaskCreationOptions.LongRunning);
                }
                ProducerRun();
                Task.WaitAll(tasks);
            }

            private void ProducerRun()
            {
                foreach (var myTask in _myTasks)
                {
                    while (_workQueue.Count > _threadCount)
                    {
                        //Thread.Sleep(0);
                    }
                    _workQueue.Enqueue(myTask);
                }
                _isComplete = true;
            }

            private void ConsumerRun()
            {
                while (true)
                {
                    if (!_workQueue.TryDequeue(out MyTask myTask))
                    {
                        if (_isComplete) break;
                        continue;
                    }
                    myTask.DoStepN(1);
                    myTask.DoStepN(2);
                    myTask.DoStepN(3);
                }
            }
        }
    }
}
