using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace MazeDemo
{
    public class MazeTaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException();
            }

            var tasklist = Task.WhenAll(
                Partitioner.Create(tasks)
                    .GetPartitions(Environment.ProcessorCount)
                    .Select(partition => Task.Run(() =>
                    {
                        using (partition)
                            while (partition.MoveNext())
                            {
                                if (partition.Current == null) continue;
                                partition.Current.DoStepN(1);
                                partition.Current.DoStepN(2);
                                partition.Current.DoStepN(3);
                            }
                    })));

            tasklist.GetAwaiter().GetResult();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var runner = new MazeTaskRunner();
            runner.ExecuteTasks(100);
        }
    }
}