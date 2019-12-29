#region using

using System;
using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Common;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class PaginationAggregator : IAggregator
	{
		private readonly List<Entry> savesList = new List<Entry>();

		public void ProcessSave(Save save)
		{
			if (savesList.Count < 100)
			{
				savesList.Add(new Entry
				{
					SaveFile = save.UUID,
					Value =
						$"{save.MachineInformation.Cpu.Name.Trim()} === {save.MachineInformation.Cpu.Vendor} === {Math.Round(save.Results.Average(r => r.Value.First(bench => bench.Benchmark.ToLowerInvariant() == "category: all").Points), 0)}"
				});
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			var list = new List<Aggregate>();
			var i = 0;

			while (i < savesList.Count)
			{
				var range = savesList.Skip(i).Take(10).ToList();
				list.Add(new Aggregate((i / 10 + 1).ToString(), "pagination", range));
				i += 10;
			}

			return list;
		}
	}
}