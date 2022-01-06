using ParallelProcessPractice.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BillDemo {
  class TaskRunner : TaskRunnerBase {
    public override void Run(IEnumerable<MyTask> tasks) {
      Parallel.ForEach(tasks, new ParallelOptions { MaxDegreeOfParallelism = 11 }, task => {
        task.DoStepN(1);
        task.DoStepN(2);
        task.DoStepN(3);
      });
    }
  }
}
