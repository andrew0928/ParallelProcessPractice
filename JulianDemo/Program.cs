using System;
using ParallelProcessPractice.Core;

namespace JulianDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new TaskRunner();
            run.ExecuteTasks(100);
        }
    }
}