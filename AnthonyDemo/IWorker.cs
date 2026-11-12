using ParallelProcessPractice.Core;
using System.Collections.Concurrent;

namespace AnthonyDemo
{
    public interface IWorker
    {
        /// <summary>
        /// 佇列來源
        /// </summary>
        ConcurrentQueue<MyTask> Queue { get; set; }

        /// <summary>
        /// 預計作業數
        /// </summary>
        int ExpectCount { get; set; }

        /// <summary>
        /// 已執行作業數
        /// </summary>
        int FinishedCount { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// 階段序號
        /// </summary>
        int Step { get; set; }

        /// <summary>
        /// 開始接收佇列
        /// </summary>
        /// <param name="startCount"></param>
        /// <returns></returns>
        void Receive(int startCount);

        /// <summary>
        /// 下一個關聯worker
        /// </summary>
        IWorker Next { get; set; }
    }
}