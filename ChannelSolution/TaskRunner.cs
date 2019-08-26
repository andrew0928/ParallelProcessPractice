using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace ChannelSolution
{
    public class TaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            var opt1 = new BoundedChannelOptions(1)
                {SingleWriter = true, SingleReader = false, AllowSynchronousContinuations = false};
            var opt2 = new BoundedChannelOptions(1)
                {SingleWriter = true, SingleReader = false, AllowSynchronousContinuations = false};
            var ch1 = Channel.CreateBounded<MyTask>(opt1);
            var ch2 = Channel.CreateBounded<MyTask>(opt2);
            var processStep1 = ProcessStep1(ch1.Writer, tasks);
            var processStep2 = ProcessStep2(ch1.Reader, ch2.Writer);
            var processStep3 = ProcessStep3(ch2.Reader);
            Task.WaitAll(processStep1, processStep2, processStep3);
        }

        private static async Task ProcessStep1(ChannelWriter<MyTask> ouput, IEnumerable<MyTask> tasks)
        {
            foreach (var task in tasks)
            {
                task.DoStepN(1);
                while (await ouput.WaitToWriteAsync())
                {
                    if (ouput.TryWrite(task)) break;
                }
            }

            ouput.Complete();
        }

        private static async Task ProcessStep2(ChannelReader<MyTask> input, ChannelWriter<MyTask> output)
        {
            while (await input.WaitToReadAsync())
            {
                if (input.TryRead(out MyTask task))
                {
                    task.DoStepN(2);
                    while (await output.WaitToWriteAsync())
                    {
                        if (output.TryWrite(task)) break;
                    }
                }

                if (input.Completion.IsCompleted) break;
            }

            output.TryComplete();
        }

        private static async Task ProcessStep3(ChannelReader<MyTask> input)
        {
            while (await input.WaitToReadAsync())
            {
                if (input.TryRead(out MyTask task))
                {
                    task.DoStepN(3);
                }

                if (input.Completion.IsCompleted) break;
            }
        }
    }
}