#region using

using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;
using CPU_Benchmark_Server_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class ByScoreAggregator : IAggregator
	{
		private readonly Dictionary<string, List<Save>> byBenchmarkResults = new Dictionary<string, List<Save>>();
		private readonly Dictionary<uint, List<Save>> byCoreResults = new Dictionary<uint, List<Save>>();

		public void ProcessSave(Save save)
		{
			foreach (var result in save.Results)
			{
				if (!byCoreResults.ContainsKey(result.Key))
				{
					byCoreResults.Add(result.Key, new List<Save> {save});
				}
				else
				{
					var inserted = false;

					for (var i = 0; i < byCoreResults[result.Key].Count; i++)
					{
						var currentSave = byCoreResults[result.Key][i];
						var eligibleResults = currentSave.Results[result.Key]
							.Where(r => !r.Benchmark.StartsWith("Category:")).ToList();
						var score = eligibleResults.Sum(r => r.Points) / eligibleResults.Count;

						var newEligibleResults = save.Results[result.Key]
							.Where(r => !r.Benchmark.StartsWith("Category:"))
							.ToList();
						var newScore = newEligibleResults.Sum(r => r.Points) / newEligibleResults.Count;

						if (newScore > score)
						{
							byCoreResults[result.Key].Insert(i, save);
							inserted = true;
							break;
						}
					}

					if (!inserted && byCoreResults[result.Key].Count < 100)
					{
						byCoreResults[result.Key].Add(save);
					}
				}

				foreach (var res in result.Value)
				{
					var key = $"{res.Benchmark} @ {result.Key} Threads";

					if (!byBenchmarkResults.ContainsKey(key))
					{
						byBenchmarkResults.Add(key, new List<Save> {save});
					}
					else
					{
						var inserted = false;

						for (var i = 0; i < byBenchmarkResults[key].Count; i++)
						{
							var currentScore = byBenchmarkResults[key][i].Results[result.Key]
								.First(r => r.Benchmark == res.Benchmark);

							if (res.Points > currentScore.Points)
							{
								byBenchmarkResults[key].Insert(i, save);
								inserted = true;
								break;
							}
						}

						if (!inserted && byBenchmarkResults[key].Count < 100)
						{
							byBenchmarkResults[key].Add(save);
						}
					}
				}
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			var list = new List<Aggregate>();

			foreach (var byCoreResult in byCoreResults)
			{
				list.Add(new Aggregate($"Threads {byCoreResult.Key}", "byCores",
					byCoreResult.Value.GetRange(0, 100).Select(s => s.UUID)));
			}

			foreach (var byBenchmarkResult in byBenchmarkResults)
			{
				list.Add(new Aggregate(byBenchmarkResult.Key, "byBenchmarks",
					byBenchmarkResult.Value.GetRange(0, 100).Select(s => s.UUID)));
			}

			return list;
		}
	}
}