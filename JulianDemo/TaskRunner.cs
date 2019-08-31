using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using ParallelProcessPractice.Core;

namespace JulianDemo
{
    public class TaskRunner : TaskRunnerBase
    {
        public override void Run(IEnumerable<MyTask> tasks)
        {
            var ch1 = Channel.CreateBounded<MyTask>(new BoundedChannelOptions(1)
                {SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true});
            var ch2 = Channel.CreateBounded<MyTask>(new BoundedChannelOptions(1)
                {SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true});
            var processStep2 = ProcessStep2(ch1.Reader, ch2.Writer);
            var processStep3 = ProcessStep3(ch2.Reader);
            var processStep1 = ProcessStep1(ch1.Writer, tasks);
            Task.WaitAll(processStep1, processStep2, processStep3);
        }

        private static async Task ProcessStep1(ChannelWriter<MyTask> writer, IEnumerable<MyTask> tasks)
        {
            var ts =tasks.Select(async t => await Task.Run(
                async () => {
                    t.DoStepN(1);
                    while (await writer.WaitToWriteAsync())
                    {
                        if (writer.TryWrite(t)) break;
                    } } 
            ));

            Task.WaitAll(ts.ToArray());
            writer.Complete();
        }

        private static async Task ProcessStep2(ChannelReader<MyTask> reader, ChannelWriter<MyTask> writer)
        {
            var tasks = new List<Task>();
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out MyTask mytask))
                {
                    var task = mytask;
                    var t = Task.Run(async () =>
                    {
                        task.DoStepN(2);
                        while (await writer.WaitToWriteAsync())
                        {
                            if (writer.TryWrite(task)) break;
                        }
                    });
                    tasks.Add(t);
                }

                if (reader.Completion.IsCompleted) break;
            }

            Task.WaitAll(tasks.ToArray());
            writer.TryComplete();
        }

        private static async Task ProcessStep3(ChannelReader<MyTask> reader)
        {
            var tasks = new List<Task>();
            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out MyTask mytask))
                {
                    var task = mytask;
                    var t =  Task.Run(() =>
                    {
                        task.DoStepN(3);
                    });
                    tasks.Add(t);
                }
                
                if (reader.Completion.IsCompleted) break;
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}