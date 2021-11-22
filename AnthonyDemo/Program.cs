using System;

namespace AnthonyDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AnthonyTaskRunner run = new AnthonyTaskRunner();
            run.ExecuteTasks(1000);
        }
    }
}