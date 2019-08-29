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
            foreach (var task in tasks)
            {
                var t = Task.Factory.StartNew(() =>
                {
                    task.DoStepN(step);
                    return task;
                });
                hohoho.Add(t);

                if (hohoho.Count >= 4)
                {
                    while (true)
                    {
                        var o = hohoho[0];
                        if (o.IsCompleted)
                        {
                            hohoho.RemoveAt(0);
                            yield return o.Result;
                        }

                        if (hohoho.Count == 0)
                        {
                            break;
                        }
                    }
                }


            }

            while (hohoho.Count != 0)
            {
                var o = hohoho[0];
                if (o.IsCompleted)
                {
                    hohoho.RemoveAt(0);
                    yield return o.Result;
                }
            }
        }
    }
}