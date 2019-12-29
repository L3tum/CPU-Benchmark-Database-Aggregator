#region using

using System.Collections.Generic;
using CPU_Benchmark_Common;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal interface IAggregator
	{
		void ProcessSave(Save save);

		IEnumerable<Aggregate> GetAggregatedResults();
	}
}