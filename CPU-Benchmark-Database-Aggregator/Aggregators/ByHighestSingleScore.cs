#region using

using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class ByHighestSingleScore : IAggregator
	{
		private readonly List<Entry> singleScoreHighest = new List<Entry>();

		public void ProcessSave(Save save)
		{
			var highestResults = save.Results.Select(result =>
				result.Value.Aggregate((b1, b2) => b1.Points > b2.Points ? b1 : b2));
			var highestResult = highestResults.Aggregate((r1, r2) => r1.Points > r2.Points ? r1 : r2);

			var inserted = false;

			for (var i = 0; i < singleScoreHighest.Count; i++)
			{
				if (double.Parse(singleScoreHighest[i].Value.Split(" === ").Last()) < highestResult.Points)
				{
					singleScoreHighest.Insert(i,
						new Entry
						{
							Value =
								$"{save.MachineInformation.Cpu.Name} === {save.MachineInformation.Cpu.Vendor} === {highestResult.Points}",
							SaveFile = save.UUID
						});

					inserted = true;
					break;
				}
			}

			if (!inserted && singleScoreHighest.Count < 100)
			{
				singleScoreHighest.Add(new Entry
				{
					Value =
						$"{save.MachineInformation.Cpu.Name} === {save.MachineInformation.Cpu.Vendor} === {highestResult.Points}",
					SaveFile = save.UUID
				});
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			return new List<Aggregate>
			{
				new Aggregate("single-score", "byHighestScore", singleScoreHighest.Take(100))
			};
		}
	}
}