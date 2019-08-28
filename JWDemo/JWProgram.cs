using ParallelProcessPractice.Core;
using System;

namespace JW
{
    class JWProgram
    {
        static void Main(string[] args) {

            TaskRunnerBase runner = new JWTaskRunner();
            runner.ExecuteTasks(1000);
        }
    }
}
