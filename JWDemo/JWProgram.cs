using ParallelProcessPractice.Core;
using System;

namespace JW
{
    class JWProgram
    {
        static void Main(string[] args) {

            TaskRunnerBase runner = new JWTaskRunnerV5();
            runner.ExecuteTasks(30);
        }
    }
}
