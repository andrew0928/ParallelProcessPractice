using System;
using System.Collections.Generic;
using System.Threading;
using ParallelProcessPractice.Core;

namespace LeviDemo
{
    public class SimpleThreadPool : IDisposable
    {
        private List<Thread> _workerThreads = new List<Thread>();

        private bool _stop_flag = false;
        private bool _cancel_flag = false;

        private TimeSpan _maxWorkerThreadTimeout = TimeSpan.FromMilliseconds(3000);
        private int _maxWorkerThreadCount = 0;

        private Queue<MyTask> _workitems = new Queue<MyTask>();
        private ManualResetEvent enqueueNotify = new ManualResetEvent(false);

        public SimpleThreadPool(int threads) {
            this._maxWorkerThreadCount = threads;
        }

        public void CreateMaxThreads() 
        {
            for (int i = 0; i < this._maxWorkerThreadCount; i = i + 1) 
            {
                this.CreateMaxThreads();
            }
        }

        private void CreateWorkerThread() {
            Thread worker = new Thread(new ThreadStart(this.DoWorkerThread));
            this._workerThreads.Add(worker);
            worker.Start();
        }

        public bool QueueUserWorkerItem(MyTask task) {
            if (this._stop_flag == true) return false;

            if (this._workitems.Count > 0 && this._workerThreads.Count < this._maxWorkerThreadCount) this.CreateWorkerThread();

            this._workitems.Enqueue(task);
            this.enqueueNotify.Set();

            return true;
        }

        public void EndPool() {
            this.EndPool(false);
        }

        public void CancelPool() 
        {
            this.EndPool(true);
        }

        public void EndPool(bool cancelQueueItem)
        {
            if (this._workerThreads.Count == 0) return;

            this._stop_flag = true;
            this._cancel_flag = cancelQueueItem;
            this.enqueueNotify.Set();

            do{
                Thread worker = this._workerThreads[0];
                worker.Join();
                this._workerThreads.Remove(worker);
            } while (this._workerThreads.Count > 0);
        }

        private void DoWorkerThread() {
            while(true) {
                while(this._workitems.Count > 0)
                {
                    MyTask task = null;
                    lock (this._workitems)
                    {
                        if (this._workitems.Count > 0) {
                            task = this._workitems.Dequeue();
                        } 
                    }

                    if (task == null) continue;

                    task.DoStepN(1);
                    task.DoStepN(2);
                    task.DoStepN(3);

                    if (this._cancel_flag == true) break;
                }

                if (this._stop_flag == true || this._cancel_flag == true) break;
                if (this.enqueueNotify.WaitOne(this._maxWorkerThreadTimeout, true) == true) continue;
                break;
            }

            this._workerThreads.Remove(Thread.CurrentThread);
        }

        public void Dispose()
        {
            this.EndPool(false);
        }
    }
}