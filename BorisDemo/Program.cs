using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParallelProcessPractice.Core;

namespace BorisDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new BorisTaskRunner(new[] { 5, 2, 3 });
            run.ExecuteTasks(100);
        }
    }

    public class Channel<T> : IDisposable
    {
        private readonly ManualResetEvent _gateway;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private object _queueSync = new object();
        private Queue<T> _queue = new Queue<T>();
        private IList<Thread> _threads;

        public delegate void ItemHandler(T status);
        public event ItemHandler ProcessItem;

        public int ChannelNumber { get; private set; }

        public Channel(int index, int concurrent)
        {
            this.ChannelNumber = index;
            this._gateway = new ManualResetEvent(false);
            _cancellationTokenSource = new CancellationTokenSource();
            _threads = Enumerable.Range(0, concurrent).Select(_ => new Thread(Process)).ToList();

            foreach (var t in _threads) t.Start();
        }

        public void Enqueue(T item)
        {
            lock (_queueSync)
            {
                _queue.Enqueue(item);
            }
            _gateway.Set();
        }

        private void Process()
        {
            T item = default(T);
            while (true)
            {
                lock (_queueSync)
                {
                    _queue.TryDequeue(out item);
                }

                if (item == null)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                        _gateway.WaitOne();
                }
                else
                {
                    ProcessItem?.Invoke(item);
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            foreach (var t in _threads) t.Join();
        }
    }

    public class BorisTaskRunner : TaskRunnerBase
    {
        private readonly int[] _stepConcurrents;


        public BorisTaskRunner(params int[] stepCocurrents)
        {
            this._stepConcurrents = stepCocurrents;
        }

        public override void Run(IEnumerable<MyTask> tasks)
        {
            var stepCnt = _stepConcurrents.Length;
            var channels = Enumerable.Range(1, stepCnt).Select(idx => new Channel<MyTask>(idx, _stepConcurrents[idx - 1])).ToList();
            channels.ForEach(c => c.ProcessItem += (item) =>
            {
                item.DoStepN(c.ChannelNumber);
                //var steps = Enumerable.Range(1, c.ChannelNumber).Select(x => $"S{x}");
                //Console.WriteLine($"job({item.ID,3}) (Thread: {Thread.CurrentThread.ManagedThreadId,3}) => {string.Join(" => ", steps)}");
                if (c.ChannelNumber + 1 <= stepCnt)
                {
                    channels[c.ChannelNumber].Enqueue(item);
                }
            });

            foreach (var t in tasks) channels[0].Enqueue(t);

            foreach (var channel in channels) channel.Dispose();
        }
    }
}

/*
##### 5 1 3
Execution Summary: BorisDemo.BorisTaskRunner, PASS

* Max WIP:
  - ALL:      12
  - Step #1:  5
  - Step #2:  1
  - Step #3:  3

* Waiting (Lead) Time:
  - Time To First Task Completed: 1432.4424 msec
  - Time To Last Task Completed:  18478.2674 msec
  - Total Waiting Time: 995158.9734 / msec, Average Waiting Time: 9951.589734

* Execute Count:
  - Total:   100
  - Success: 100
  - Failure: 0
  - Complete Step #1: 100
  - Complete Step #2: 100
  - Complete Step #3: 100

##### 5 2 3
Execution Summary: BorisDemo.BorisTaskRunner, PASS

* Max WIP:
  - ALL:      12
  - Step #1:  6
  - Step #2:  2
  - Step #3:  3

* Waiting (Lead) Time:
  - Time To First Task Completed: 1433.2289 msec
  - Time To Last Task Completed:  18352.0007 msec
  - Total Waiting Time: 963539.2711 / msec, Average Waiting Time: 9635.392711

* Execute Count:
  - Total:   100
  - Success: 100
  - Failure: 0
  - Complete Step #1: 100
  - Complete Step #2: 100
  - Complete Step #3: 100

##### 5 3 3
Execution Summary: BorisDemo.BorisTaskRunner, PASS

* Max WIP:
  - ALL:      13
  - Step #1:  5
  - Step #2:  3
  - Step #3:  4

* Waiting (Lead) Time:
  - Time To First Task Completed: 1432.525 msec
  - Time To Last Task Completed:  18351.8497 msec
  - Total Waiting Time: 966441.1335 / msec, Average Waiting Time: 9664.411335

* Execute Count:
  - Total:   100
  - Success: 100
  - Failure: 0
  - Complete Step #1: 100
  - Complete Step #2: 100
  - Complete Step #3: 100
*/
