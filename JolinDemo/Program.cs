using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParallelProcessPractice.Core;

namespace JolinDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int totalStep = 3;
            var concurrentPerStep = new int[3] { 5, 3, 3 };
            TaskRunnerBase run = new JolinTaskRunner(totalStep, concurrentPerStep);
            run.ExecuteTasks(100);
        }
    }

    public class JolinTaskRunner : TaskRunnerBase
    {
        private TaskPipeline _pipeline;
        private int _totalStep;

        public JolinTaskRunner(int totalStep, params int [] concurrentPerStep)
        {
            _totalStep = totalStep;

            TaskPipeline previousPipeline = null;
            for (int i = 1; i <= _totalStep; i++)
            {
                var p = new TaskPipeline(i, concurrentPerStep[i - 1]);

                if (i == 1)
                {
                    _pipeline = p;
                }

                if (previousPipeline != null)
                    previousPipeline.NextPipeline = p;

                previousPipeline = p;
            }
        }

        public override void Run(IEnumerable<MyTask> tasks)
        {
            foreach (var t in tasks)
            {
                _pipeline.AddTask(t);
            }


            _pipeline.WaitFinished().Wait();
        }
    }
}