using System;
using ParallelProcessPractice.Core;

namespace EPDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new EPTaskRunner();
            run.ExecuteTasks(100);
        }
    }
}
