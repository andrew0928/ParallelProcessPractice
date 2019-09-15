using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NathanDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            NathanTaskRunner run = new NathanTaskRunner();
            run.ExecuteTasks(30);
            Console.ReadLine();
        }
    }
}