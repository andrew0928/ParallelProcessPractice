using System;
using ParallelProcessPractice.Core;

namespace GuluDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new GuluTaskRunner();
            run.ExecuteTasks(1000);
        }
    }
}
