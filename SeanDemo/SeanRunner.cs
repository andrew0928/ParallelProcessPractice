using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace SeanDemo
{
    public class SeanRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            var maxCount = tasks.Count();
            using (var pipe = new PipeLineHead<MyTask>(tasks, x => x.DoStepN(1)))
            {
                pipe.SetNextPipeLine(new PipeLine<MyTask>(maxCount, x => x.DoStepN(2)))
                    .SetNextPipeLine(new PipeLine<MyTask>(maxCount, x => x.DoStepN(3)));

                pipe.StartPipeLine();
                SpinWait.SpinUntil(() => pipe.CheckCompleted()); 
            }
        }
    }

    public class PipeLineHead<T> : IDisposable where T : class
    {
        private readonly IEnumerable<T> _task;
        private readonly Action<T> _action;
        public PipeLine<T> NextPipeLine { get; private set; }

        public PipeLineHead(IEnumerable<T> tasks, Action<T> action)
        {
            _task = tasks;
            _action = action;
        }

        public PipeLine<T> SetNextPipeLine(PipeLine<T> pipeLine)
        {
            NextPipeLine = pipeLine;
            return pipeLine;
        }

        public bool CheckCompleted()
        {
            var result = true;
            var nextPipeLine = NextPipeLine;
            while (nextPipeLine != null)
            {
                result &= nextPipeLine.IsCompleted;
                nextPipeLine = nextPipeLine.NextPipeLine;
            }

            return result;
        }

        public void StartPipeLine()
        {
            foreach (var task in _task)
            {
                _action(task);
                NextPipeLine?.AddTask(task);
            }
        }

        public void Dispose()
        {
            var nextPipeLine = NextPipeLine;
            while (nextPipeLine != null)
            {
                nextPipeLine.Dispose();
                nextPipeLine = nextPipeLine.NextPipeLine;
            }
        }
    }


    public class PipeLine<T> : IDisposable where T : class
    {
        private readonly BlockingCollection<T> _blockingCollection;
        private readonly Action<T> _action;
        private readonly int _maxTaskCount;
        public PipeLine<T> NextPipeLine { get; private set; }

        public PipeLine(int maxTaskTaskCount, Action<T> action)
        {
            _maxTaskCount = maxTaskTaskCount;
            _blockingCollection = new BlockingCollection<T>(maxTaskTaskCount);
            _action = action;
            StartPullingTask();
        }

        public void AddTask(T task)
        {
            _blockingCollection.Add(task);
        }

        public PipeLine<T> SetNextPipeLine(PipeLine<T> pipeLine)
        {
            NextPipeLine = pipeLine;
            return pipeLine;
        }

        private void StartPullingTask()
        {
            Task.Run(() =>
            {
                var currentCount = 0;
                while (_blockingCollection.IsAddingCompleted == false)
                {
                    var task = _blockingCollection.Take();
                    _action(task);
                    NextPipeLine?.AddTask(task);
                    currentCount++;
                    if (currentCount == _maxTaskCount)
                        _blockingCollection.CompleteAdding();
                }
            });
        }

        public bool IsCompleted => _blockingCollection.IsCompleted;

        public void Dispose()
        {
            _blockingCollection?.Dispose();
        }
    }
}
