#region using

using System;
using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class PaginationAggregator : IAggregator
	{
		private readonly List<Save> savesList = new List<Save>();

		public void ProcessSave(Save save)
		{
			if (savesList.Count < 100)
			{
				savesList.Add(save);
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			var list = new List<Aggregate>();
			var i = 0;

			Console.WriteLine("Saves: " + savesList.Count);

			while (i < savesList.Count)
			{
				Console.WriteLine(i);
				var range = savesList.Skip(i).Take(10).ToList().Select(s => s.UUID);
				Console.WriteLine(range.Count());
				list.Add(new Aggregate((i % 10 + 1).ToString(), "pagination", range));
				i += 10;
				Console.WriteLine(i + " of " + savesList.Count);
			}

			return list;
		}
	}
}