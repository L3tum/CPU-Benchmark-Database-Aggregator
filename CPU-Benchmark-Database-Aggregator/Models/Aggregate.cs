#region using

using System.Collections.Generic;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Models
{
	internal class Aggregate
	{
		public Aggregate(string name, string category, IEnumerable<Entry> entries)
		{
			Name = name;
			Category = category;
			Entries = entries;
		}

		public string Name { get; set; }

		public string Category { get; set; }

		public IEnumerable<Entry> Entries { get; set; }
	}

	internal class Entry
	{
		public string Value { get; set; }

		public string SaveFile { get; set; }
	}
}