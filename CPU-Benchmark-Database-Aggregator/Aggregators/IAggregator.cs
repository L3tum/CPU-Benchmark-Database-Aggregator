using CPU_Benchmark_Database_Aggregator.Models;
using CPU_Benchmark_Server_Aggregator.Models;
using System.Collections.Generic;

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
    internal interface IAggregator
    {
        void ProcessSave(Save save);

        IEnumerable<Aggregate> GetAggregatedResults();
    }
}
