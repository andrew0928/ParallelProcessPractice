using System;
using ParallelProcessPractice.Core;

namespace PhoenixDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			TaskRunnerBase runner = new PhoenixTaskRunner();
			runner.ExecuteTasks(30);
		}
	}
}
