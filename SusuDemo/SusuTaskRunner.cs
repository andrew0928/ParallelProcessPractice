using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace SusuDemo
{
    public class SusuTaskRunner : TaskRunnerBase
    {
        private int blockingCollectionSize = 10;

        public override void Run(IEnumerable<MyTask> tasks)
        {
            this.BlockingCollectionRun(tasks);
        }

        /// <summary>
        /// 批次處理
        /// </summary>
        /// <param name="tasks">任務列表</param>
        private void BatchRun(IEnumerable<MyTask> tasks)
        {
            foreach (var task in tasks)
            {
                task.DoStepN(1);
            }

            foreach (var task in tasks)
            {
                task.DoStepN(2);
            }

            foreach (var task in tasks)
            {
                task.DoStepN(3);
            }
        }

        /// <summary>
        /// 串流處理
        /// </summary>
        /// <param name="tasks">任務列表</param>
        private void StreamRun(IEnumerable<MyTask> tasks)
        {
            foreach (var task in tasks)
            {
                task.DoStepN(1);
                task.DoStepN(2);
                task.DoStepN(3);
            }
        }

        /// <summary>
        /// 管線處理
        /// </summary>
        /// <param name="tasks">任務列表</param>
        private void PipelinRun(IEnumerable<MyTask> tasks)
        {
            this.PipelinProcessStep(this.PipelinProcessStep(this.PipelinProcessStep(tasks, 1), 2), 3).ToList();
        }

        private IEnumerable<MyTask> PipelinProcessStep(IEnumerable<MyTask> tasks, int step)
        {
            foreach (var task in tasks)
            {
                task.DoStepN(step);
                yield return task;
            }
        }

        /// <summary>
        /// 管線處理(Async)
        /// </summary>
        /// <param name="tasks">任務列表</param>
        private void PipelineAsyncRun(IEnumerable<MyTask> tasks)
        {
            this.PipelinAsyncProcessStep(this.PipelinAsyncProcessStep(this.PipelinAsyncProcessStep(tasks, 1), 2), 3).ToList();
        }

        private IEnumerable<MyTask> PipelinAsyncProcessStep(IEnumerable<MyTask> tasks, int step)
        {
            Task<MyTask> taskResult = null;

            foreach (var task in tasks)
            {
                if (taskResult != null) yield return taskResult.GetAwaiter().GetResult();
                taskResult = Task.Run<MyTask>(() => 
                {
                    task.DoStepN(step);
                    return task;
                });
            }

            if (taskResult != null) yield return taskResult.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 管線處理(BlockingCollection)
        /// </summary>
        /// <param name="tasks">任務列表</param>
        private void BlockingCollectionRun(IEnumerable<MyTask> tasks)
        {
            this.BlockingCollectionProcessStep(this.BlockingCollectionProcessStep(this.BlockingCollectionProcessStep(tasks, 1), 2), 3).ToList();
        }

        private IEnumerable<MyTask> BlockingCollectionProcessStep(IEnumerable<MyTask> tasks, int step)
        {
            BlockingCollection<MyTask> taskResultCollection = new BlockingCollection<MyTask>(blockingCollectionSize);

            Task.Run(() => 
            {
                foreach (var task in tasks)
                {
                    task.DoStepN(step);
                    taskResultCollection.Add(task);
                }

                taskResultCollection.CompleteAdding();
            });

            foreach (var task in taskResultCollection.GetConsumingEnumerable())
            {
                yield return task;
            }
        }
    }
}
