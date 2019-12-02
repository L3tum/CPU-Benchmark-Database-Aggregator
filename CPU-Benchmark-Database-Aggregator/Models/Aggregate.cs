#region using

using System.Collections.Generic;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Models
{
	internal class Aggregate
	{
		public Aggregate(string name, string category, IEnumerable<string> resultSaveUUIDs)
		{
			Name = name;
			Category = category;
			ResultSaveUUIDs = resultSaveUUIDs;
		}

		public string Name { get; set; }

		public string Category { get; set; }

		public IEnumerable<string> ResultSaveUUIDs { get; set; }
	}
}