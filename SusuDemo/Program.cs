using System;
using ParallelProcessPractice.Core;

namespace SusuDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskRunnerBase run = new SusuTaskRunner();
            run.ExecuteTasks(100);
        }
    }
}
