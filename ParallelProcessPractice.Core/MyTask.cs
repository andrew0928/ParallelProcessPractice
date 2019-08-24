//#define ENABLE_STEP_CONTEXT

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ParallelProcessPractice.Core
{
#if (ENABLE_STEP_CONTEXT)
    public class MyTaskProcessContext:IDisposable
    {
        private readonly TaskRunnerBase.RunnerContext _context = null;

        public readonly int CurrentStep = 0;

        private string _buffer_handler = null;

        internal MyTaskProcessContext(int step, TaskRunnerBase.RunnerContext context)
        {
            if (step < 1) throw new ArgumentOutOfRangeException();
            if (step > PracticeSettings.TASK_TOTAL_STEPS) throw new ArgumentOutOfRangeException();

            this.CurrentStep = step;
            this._context = context;

            //Thread.Sleep(PracticeSettings.TASK_STEP_CONTEXT_INIT_DURATION[this.CurrentStep]);
            //Interlocked.Add(ref this._context._allocated_memory_size, PracticeSettings.TASK_STEP_CONTEXT_BUFFER[this.CurrentStep]);
            this._buffer_handler = this._context.AllocateBuffer(PracticeSettings.TASK_STEP_CONTEXT_BUFFER[this.CurrentStep]);
        }

        public void Dispose()
        {
            //Interlocked.Add(ref this._context._allocated_memory_size, 0 - PracticeSettings.TASK_STEP_CONTEXT_BUFFER[this.CurrentStep]);
            this._context.FreeBuffer(this._buffer_handler);
            this._buffer_handler = null;
        }
    }
#endif

    public class MyTask
    {
        public int ID { get; private set; }

        public int CurrentStep { get; private set; } = 0;


        private readonly TaskRunnerBase.RunnerContext _context = null;

        private string _work_buffer_handle = null;
        private string _globel_buffer_handle = null;

        internal MyTask(TaskRunnerBase.RunnerContext context)
        {
            this._context = context;
            this.ID = this._context.GetSeed();
        }

#if (ENABLE_STEP_CONTEXT)
        public void DoStepN(int step, MyTaskProcessContext step_context = null)
#else
        public void DoStepN(int step)
#endif
        {
            if (step > PracticeSettings.TASK_TOTAL_STEPS) throw new ArgumentOutOfRangeException();
            if (this.CurrentStep != (step - 1)) throw new InvalidOperationException();

#if (ENABLE_STEP_CONTEXT)
            if (step_context == null || step_context.CurrentStep != step) throw new InvalidOperationException();
#endif

            this._context.DoStep(this.ID, step, () => 
            {
                if (step == 1) this._globel_buffer_handle = this._context.AllocateBuffer(PracticeSettings.TASK_STEPS_BUFFER[0]);

                this._work_buffer_handle = this._context.AllocateBuffer(PracticeSettings.TASK_STEPS_BUFFER[step]);

                Thread.Sleep(PracticeSettings.TASK_STEPS_DURATION[step]);
                this.CurrentStep = step;

                this._context.FreeBuffer(this._work_buffer_handle);
                this._work_buffer_handle = null;

                if (step == 3)
                {
                    this._context.FreeBuffer(this._globel_buffer_handle);
                    this._globel_buffer_handle = null;
                }
            });
        }

    }
}
