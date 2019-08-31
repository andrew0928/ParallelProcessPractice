using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ParallelProcessPractice.Core
{
    public abstract class TaskRunnerBase
    {
        internal class RunnerContext
        {
            private int _seed = 0;

            private Semaphore[] _flags = new Semaphore[PracticeSettings.TASK_TOTAL_STEPS + 1]
            {
                null,
                new Semaphore(PracticeSettings.TASK_STEPS_CONCURRENT_LIMIT[1], PracticeSettings.TASK_STEPS_CONCURRENT_LIMIT[1]),
                new Semaphore(PracticeSettings.TASK_STEPS_CONCURRENT_LIMIT[2], PracticeSettings.TASK_STEPS_CONCURRENT_LIMIT[2]),
                new Semaphore(PracticeSettings.TASK_STEPS_CONCURRENT_LIMIT[3], PracticeSettings.TASK_STEPS_CONCURRENT_LIMIT[3])
            };

            private Dictionary<int, int> _thread_last_step_map = new Dictionary<int, int>();
            internal int _dostep_thread_step_switch_count = 0;

            internal int[] _dostep_enter_counts = { 0, 0, 0, 0 };
            internal int[] _dostep_exit_counts = { 0, 0, 0, 0 };

            private int[] _dostep_wip_current = { 0, 0, 0, 0 };
            internal int[] _dostep_wip_max = { 0, 0, 0, 0 };

            private Dictionary<int, (int seed, int step)> _thread_task_step_map = new Dictionary<int, (int seed, int step)>();

            private string[] _step_context_buffer_handlers = { null, null, null, null };

            private object _syncroot = new object();

            private void EnterStep(int seed, int step)
            {
                lock (this._syncroot)
                {
                    if (Interlocked.Increment(ref this._dostep_enter_counts[step]) == 1)
                    {
                        // first
                        this._step_context_buffer_handlers[step] = this.AllocateBuffer(PracticeSettings.TASK_STEP_CONTEXT_BUFFER[step]);
                    }

                    int mtid = Thread.CurrentThread.ManagedThreadId;

                    // thread affinity simulation
                    if (this._thread_last_step_map.ContainsKey(mtid) == false || this._thread_last_step_map[mtid] != step) Interlocked.Increment(ref this._dostep_thread_step_switch_count);
                    this._thread_last_step_map[mtid] = step;
                    this._thread_task_step_map[mtid] = (seed, step);



                    // sync wip statistics
                    this._dostep_wip_current[step] = this._dostep_enter_counts[step] - this._dostep_exit_counts[step];
                    this._dostep_wip_current[0] = this._dostep_enter_counts[1] - this._dostep_exit_counts[3];

                    this._dostep_wip_max[step] = Math.Max(this._dostep_wip_current[step], this._dostep_wip_max[step]);
                    this._dostep_wip_max[0] = Math.Max(this._dostep_wip_current[0], this._dostep_wip_max[0]);
                }
            }
            private void ExitStep(int seed, int step)
            {
                lock (this._syncroot)
                {
                    int mtid = Thread.CurrentThread.ManagedThreadId;

                    if (Interlocked.Increment(ref this._dostep_exit_counts[step]) == 100)
                    {
                        // last task
                        this.FreeBuffer(this._step_context_buffer_handlers[step]);
                    }

                    this._thread_task_step_map.Remove(mtid);


                    // sync wip statistics
                    this._dostep_wip_current[step] = this._dostep_enter_counts[step] - this._dostep_exit_counts[step];
                    this._dostep_wip_current[0] = this._dostep_enter_counts[1] - this._dostep_exit_counts[3];

                    this._dostep_wip_max[step] = Math.Max(this._dostep_wip_current[step], this._dostep_wip_max[step]);
                    this._dostep_wip_max[0] = Math.Max(this._dostep_wip_current[0], this._dostep_wip_max[0]);
                }
            }

            private long _allocated_memory_size = 0;
            internal long _allocated_memory_peak = 0;
            private Dictionary<string, long> _allocated_heap = new Dictionary<string, long>();
            //private object _allocation_syncroot = new object();

            internal string AllocateBuffer(long size)
            {
                string handle = Guid.NewGuid().ToString("N");
                lock (this._syncroot)
                {
                    this._allocated_heap.Add(handle, size);
                    this._allocated_memory_size += size;
                    this._allocated_memory_peak = Math.Max(this._allocated_memory_peak, this._allocated_memory_size);
                }
                return handle;
            }
            internal bool FreeBuffer(string handle)
            {
                if (string.IsNullOrEmpty(handle))
                {
                    Console.Error.WriteLine("- warning:   can not free (handle: NULL) allocated buffer.");
                    return false;
                }
                if (this._allocated_heap.ContainsKey(handle) == false) return false;

                lock (this._syncroot)
                {
                    if (this._allocated_heap.ContainsKey(handle) == false) return false;

                    this._allocated_memory_size -= this._allocated_heap[handle];
                    this._allocated_heap.Remove(handle);
                }

                return true;
            }

            internal Stopwatch _execution_timer = new Stopwatch();

            internal TimeSpan _time_to_first_task_complete = TimeSpan.Zero;
            internal TimeSpan _time_to_last_task_complete = TimeSpan.Zero;
            internal TimeSpan _total_leadtime = TimeSpan.Zero;


            internal int GetSeed()
            {
                return Interlocked.Increment(ref this._seed);
            }

            internal void DoStep(int taskID, int step, Action action)
            {
                if (step <= 0 || step > PracticeSettings.TASK_TOTAL_STEPS) throw new ArgumentOutOfRangeException();


                this._flags[step].WaitOne();
                {
                    this.EnterStep(taskID, step);

                    action();

                    this.ExitStep(taskID, step);
                }
                this._flags[step].Release();

                if (step == 3)
                {
                    if (this._time_to_first_task_complete == TimeSpan.Zero) this._time_to_first_task_complete = this._execution_timer.Elapsed;
                    this._time_to_last_task_complete = this._execution_timer.Elapsed;
                    this._total_leadtime += this._execution_timer.Elapsed;
                }
            }

            internal void MonitorWorker(string path, ManualResetEvent readyRunWait, CancellationTokenSource cancel)
            {
                int threads_count = 30;
                int monitor_sample_time = 30;
                int[] idmap = new int[threads_count];
                HashSet<int> idset = new HashSet<int>();

                using (TextWriter output = new StreamWriter(path, false, Encoding.ASCII))
                {

                    // write header: column names

                    output.Write("TS,MEM,WIP_ALL,WIP1,WIP2,WIP3");
                    output.Write(",THREADS_COUNT");
                    output.Write(",ENTER1,ENTER2,ENTER3");
                    output.Write(",EXIT1,EXIT2,EXIT3");

                    for (int i = 0; i < threads_count; i++) output.Write($",T{i + 1}");
                    output.WriteLine();


                    //for (int count = 1; cts.IsCancellationRequested == false; count++)
                    int count = 1;
                    bool stop = false;
                    do
                    {
                        stop = cancel.IsCancellationRequested;
                        int[] x = null;
                        lock (this._syncroot) x = this._thread_task_step_map.Keys.ToArray();
                        foreach (int mtid in x)
                        {
                            if (idset.Contains(mtid) == false)
                            {
                                idmap[idset.Count] = mtid;
                                idset.Add(mtid);
                            }
                        }


                        output.Write($"{this._execution_timer.ElapsedMilliseconds},{this._allocated_memory_size},{this._dostep_wip_current[0]},{this._dostep_wip_current[1]},{this._dostep_wip_current[2]},{this._dostep_wip_current[3]}");
                        output.Write($",{this._thread_task_step_map.Count}");
                        output.Write($",{this._dostep_enter_counts[1]},{this._dostep_enter_counts[2]},{this._dostep_enter_counts[3]}");
                        output.Write($",{this._dostep_exit_counts[1]},{this._dostep_exit_counts[2]},{this._dostep_exit_counts[3]}");


                        for (int i = 0; i < idmap.Length; i++)
                        {
                            int mtid = idmap[i];
                            if (mtid == 0 || this._thread_task_step_map.ContainsKey(mtid) == false || this._thread_task_step_map[mtid].seed == 0)
                            {
                                output.Write(",");
                            }
                            else
                            {
                                output.Write($",{this._thread_task_step_map[mtid].seed}#{this._thread_task_step_map[mtid].step}");
                            }
                        }
                        output.WriteLine();

                        readyRunWait.Set();
                        SpinWait.SpinUntil(() => { return this._execution_timer.ElapsedMilliseconds >= count * monitor_sample_time; });

                        output.Flush();
                        count++;
                    } while (stop == false);
                }

            }
        }


        private RunnerContext _context = null;

        public TaskRunnerBase()
        {
            this._context = new RunnerContext()
            {

            };
        }

        public abstract void Run(IEnumerable<MyTask> tasks);



        private IEnumerable<MyTask> CreateTasks(int taskCount)
        {
            for (int i = 0; i < taskCount; i++)
            {
                yield return new MyTask(this._context);
            }
        }

#if (ENABLE_PROCESS_CONTEXT)
        public MyTaskProcessContext CreateTaskProcessContext(int step)
        {
            return new MyTaskProcessContext(step, this._context);
        }
#endif

        public void ExecuteTasks(int taskCount = 1000)
        {
                      
            Exception occurs = null;

            CancellationTokenSource cancel = new CancellationTokenSource();
            

            //bool _stop = false;
            ManualResetEvent ready = new ManualResetEvent(false);
            Thread monitor = new Thread(() =>
            {
                this._context.MonitorWorker(
                    $"runner-result-{DateTime.Now:yyyy-MMdd-HHmmss}-{this.GetType().FullName}.csv",
                    ready, 
                    cancel);
            });
            

            try
            {
                this._context._execution_timer.Restart();
                monitor.Start();
                ready.WaitOne();

                this.Run(CreateTasks(taskCount));
            }
            catch (Exception ex)
            {
                occurs = ex;
            }


            //_stop = true;
            cancel.Cancel();
            monitor.Join();


            Console.Error.WriteLine();
            Console.Error.WriteLine( "Execution Summary: {0}, {1}", this.GetType().FullName, (this._context._dostep_exit_counts[3] == taskCount) ? ("PASS") : ("FAIL"));

            Console.Error.WriteLine();
            Console.Error.WriteLine($"* Max WIP:");
            Console.Error.WriteLine($"  - ALL:      {this._context._dostep_wip_max[0]}");
            Console.Error.WriteLine($"  - Step #1:  {this._context._dostep_wip_max[1]}");
            Console.Error.WriteLine($"  - Step #2:  {this._context._dostep_wip_max[2]}");
            Console.Error.WriteLine($"  - Step #3:  {this._context._dostep_wip_max[3]}");

            Console.Error.WriteLine();
            Console.Error.WriteLine($"* Used Resources:");
            Console.Error.WriteLine($"  - Memory Usage (Peak):  {this._context._allocated_memory_peak}");
            Console.Error.WriteLine($"  - Context Switch Count: {this._context._dostep_thread_step_switch_count}");

            Console.Error.WriteLine();
            Console.Error.WriteLine($"* Waiting (Lead) Time:");
            Console.Error.WriteLine($"  - Time To First Task Completed: {this._context._time_to_first_task_complete.TotalMilliseconds} msec");
            Console.Error.WriteLine($"  - Time To Last Task Completed:  {this._context._time_to_last_task_complete.TotalMilliseconds} msec");
            Console.Error.WriteLine($"  - Total Waiting Time: {this._context._total_leadtime.TotalMilliseconds} / msec, Average Waiting Time: {this._context._total_leadtime.TotalMilliseconds / taskCount}");

            Console.Error.WriteLine();
            Console.Error.WriteLine($"* Execute Count:");
            Console.Error.WriteLine($"  - Total:   {taskCount}");
            Console.Error.WriteLine($"  - Success: {this._context._dostep_exit_counts[3]}");
            Console.Error.WriteLine($"  - Failure: {taskCount - this._context._dostep_exit_counts[3]}");

            Console.Error.WriteLine($"  - Complete Step #1: {this._context._dostep_exit_counts[1]}");
            Console.Error.WriteLine($"  - Complete Step #2: {this._context._dostep_exit_counts[2]}");
            Console.Error.WriteLine($"  - Complete Step #3: {this._context._dostep_exit_counts[3]}");

            if (occurs != null)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("".PadRight(80, '-'));
                Console.Error.WriteLine($"- Exception: {occurs}");
            }
        }
    }

}
