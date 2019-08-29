using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPDemo
{
    public class EPTaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            foreach (var task in GogoNstepAsync(GogoNstepAsync(GogoNstepAsync(tasks, 1), 2), 3))
            {
                ;
            }
        }

        public static IEnumerable<MyTask> GogoNstepAsync(IEnumerable<MyTask> tasks, int step)
        {
            List<Task<MyTask>> hohoho = new List<Task<MyTask>>();
            int index = 0;
            foreach (var task in tasks)
            {
                var t = Task.Factory.StartNew(() =>
                {
                    task.DoStepN(step);
                    return task;
                });
                hohoho.Add(t);

                if (hohoho[index].IsCompleted)
                {
                    hohoho.RemoveAt(index);
                    yield return t.GetAwaiter().GetResult();
                }
                index += 1;
            }            

            while (hohoho.Count != 0)
            {
                for (int i = 0; i < hohoho.Count; i++)
                {
                    var oh = hohoho[i];
                    if (oh.IsCompleted)
                    {
                        hohoho.RemoveAt(i);
                        yield return oh.GetAwaiter().GetResult();
                    }
                }
            }
        }
    }
}