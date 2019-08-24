using System;
using System.Collections.Generic;
using System.Text;

namespace ParallelProcessPractice.Core
{
    internal static class PracticeSettings
    {
        public const int TASK_TOTAL_STEPS = 3;

        public const int WIP_WORKSET_SIZE = 128; //256 * 1024 * 1024;

        public static readonly int[] TASK_STEPS_BUFFER =
        {
            1024,   // global buffer, from step1 ~ step3
            512,
            0,
            384
        };

        //public static readonly int[] TASK_STEP_CONTEXT_INIT_DURATION =
        //{
        //    0,
        //    1000,
        //    1000,
        //    1000
        //};

        public static readonly int[] TASK_STEP_CONTEXT_BUFFER =
        {
            0,
            4 * 1024,
            4 * 1024,
            4 * 1024
        };

        public static readonly int[] TASK_STEPS_DURATION =
        {
            0,  // STEP 0, useless
            867,
            132,
            430,
        };

        public static readonly int[] TASK_STEPS_CONCURRENT_LIMIT =
        {
            0,  // STEP 0, useless
            5,
            3,
            3
        };
    }
}
