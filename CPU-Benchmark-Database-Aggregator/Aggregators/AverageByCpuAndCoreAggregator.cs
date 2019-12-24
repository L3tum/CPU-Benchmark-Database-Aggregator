#region using

using System;
using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class AverageByCpuAndCoreAggregator : IAggregator
	{
		// Cores -> CPU Name -> List of points
		private readonly Dictionary<uint, Dictionary<string, List<double>>> scores =
			new Dictionary<uint, Dictionary<string, List<double>>>();

		public void ProcessSave(Save save)
		{
			foreach (var keyValuePair in save.Results)
			{
				if (!scores.ContainsKey(keyValuePair.Key))
				{
					scores.Add(keyValuePair.Key, new Dictionary<string, List<double>>());
				}

				if (!scores[keyValuePair.Key].ContainsKey(save.MachineInformation.Cpu.Name))
				{
					scores[keyValuePair.Key].Add(save.MachineInformation.Cpu.Name, new List<double>());
				}

				scores[keyValuePair.Key][save.MachineInformation.Cpu.Name].Add(keyValuePair.Value
					.First(benchmark => benchmark.Benchmark.ToLowerInvariant() == "category: all").Points);
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			var aggregates = new List<Aggregate>();

			foreach (var keyValuePair in scores)
			{
				aggregates.Add(new Aggregate(keyValuePair.Key.ToString(), "averageByCoreCount",
					keyValuePair.Value.Select(kvp => new Entry
						{Value = $"{kvp.Key} === {Math.Round(kvp.Value.Average(), 0)}"})));
			}

			return aggregates;
		}
	}
}