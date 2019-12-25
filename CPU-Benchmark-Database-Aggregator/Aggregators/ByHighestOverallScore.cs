#region using

using System;
using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class ByHighestOverallScore : IAggregator
	{
		private readonly List<Entry> scoreHighest = new List<Entry>();

		public void ProcessSave(Save save)
		{
			var highestResult = Math.Round(save.Results.Average(result =>
				result.Value.First(benchmark => benchmark.Benchmark.ToLowerInvariant() == "category: all").Points), 0);

			var inserted = false;

			for (var i = 0; i < scoreHighest.Count; i++)
			{
				if (double.Parse(scoreHighest[i].Value.Split(" === ").Last()) < highestResult)
				{
					scoreHighest.Insert(i,
						new Entry
						{
							Value =
								$"{save.MachineInformation.Cpu.Name} === {save.MachineInformation.Cpu.Vendor} === {highestResult}",
							SaveFile = save.UUID
						});

					inserted = true;
					break;
				}
			}

			if (!inserted && scoreHighest.Count < 100)
			{
				scoreHighest.Add(new Entry
				{
					Value =
						$"{save.MachineInformation.Cpu.Name} === {save.MachineInformation.Cpu.Vendor} === {highestResult}",
					SaveFile = save.UUID
				});
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			return new List<Aggregate>
			{
				new Aggregate("overall-score", "byHighestScore", scoreHighest.Take(100))
			};
		}
	}
}