#region using

using System.Collections.Generic;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class PaginationAggregator : IAggregator
	{
		private readonly List<string> savesList = new List<string>();

		public void ProcessSave(Save save)
		{
			savesList.Add(save.UUID);
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			var list = new List<Aggregate>();
			var i = 0;

			while (i < savesList.Count)
			{
				var range = savesList.GetRange(i, i + 10 >= savesList.Count ? savesList.Count - i - 1 : 10);
				list.Add(new Aggregate((i % 10 + 1).ToString(), "pagination", range));
				i += 10;
			}

			return list;
		}
	}
}