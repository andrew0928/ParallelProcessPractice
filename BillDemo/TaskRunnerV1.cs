using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BillDemo {
  class TaskRunnerV1 : TaskRunnerBase {
    public override void Run(IEnumerable<MyTask> tasks) {

      Task t0 = Task.Run(async () => { foreach (var task in tasks) { await queues[1].Writer.WriteAsync(task); } queues[1].Writer.Complete(); });
      Task t1 = DoAllStepNAsync(1);
      Task t2 = DoAllStepNAsync(2);
      Task t3 = DoAllStepNAsync(3);

      //Task.WaitAll(tasks.Select(t => Task.Run(async () =>
      //{
      //  await this.queues[1].Writer.WriteAsync(t);
      //})).ToArray());

      //queues[1].Writer.Complete();

      Task.WaitAll(t1, t2, t3);
    }

    private Channel<MyTask>[] queues = new Channel<MyTask>[3 + 1]
    {
            null,
            Channel.CreateBounded<MyTask>(5),
            Channel.CreateUnbounded<MyTask>(),
            Channel.CreateUnbounded<MyTask>(),
    };

    private async Task DoAllStepNAsync(int step) {
      bool last = (step == 3);
      List<Task> ts = new List<Task>();
      while (await queues[step].Reader.WaitToReadAsync()) {
        while (queues[step].Reader.TryRead(out MyTask task)) {
          ts.Add(Task.Run(async () =>
          {
            task.DoStepN(step);
            if (!last)
              await queues[step + 1].Writer.WriteAsync(task);
          }));
        }
      }
      Task.WaitAll(ts.ToArray());
      if (!last)
        queues[step + 1].Writer.Complete();
    }
  }
}
