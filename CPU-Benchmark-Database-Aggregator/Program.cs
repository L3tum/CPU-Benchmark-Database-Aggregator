using System;
using System.IO;

namespace CPU_Benchmark_Database_Aggregator
{
    class Program
    {
        static void Main(string[] args)
        {
            var saves = Directory.GetFiles(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") + "/saves");
        }
    }
}
