#region using

using System.Collections.Generic;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class ByHighestFrequency : IAggregator
	{
		private readonly List<Save> allCoreHighest = new List<Save>();
		private readonly List<Save> singleCoreHighest = new List<Save>();


		public void ProcessSave(Save save)
		{
			if (save.MachineInformation.Cpu.MaxClockSpeed == 0)
			{
				return;
			}

			var inserted = false;

			for (var i = 0; i < singleCoreHighest.Count; i++)
			{
				if (save.MachineInformation.Cpu.MaxClockSpeed >
				    singleCoreHighest[i].MachineInformation.Cpu.MaxClockSpeed)
				{
					singleCoreHighest.Insert(i, save);
					inserted = true;
					break;
				}
			}

			if (!inserted && singleCoreHighest.Count < 100)
			{
				singleCoreHighest.Add(save);
			}

			if (save.MachineInformation.Cpu.Cores.Count == 0)
			{
				return;
			}

			var mcScore = save.MachineInformation.Cpu.Cores.Sum(c => c.MaxClockSpeed);

			if (mcScore == 0)
			{
				return;
			}

			inserted = false;

			for (var i = 0; i < allCoreHighest.Count; i++)
			{
				var currentMCScore = allCoreHighest[i].MachineInformation.Cpu.Cores.Sum(c => c.MaxClockSpeed);

				if (mcScore > currentMCScore)
				{
					allCoreHighest.Insert(i, save);
					inserted = true;
					break;
				}
			}

			if (!inserted && allCoreHighest.Count < 100)
			{
				allCoreHighest.Add(save);
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			return new List<Aggregate>
			{
				new Aggregate("single-core", "byHighestFrequency", singleCoreHighest.Select(s => s.UUID)),
				new Aggregate("all-core", "byHighestFrequency", allCoreHighest.Select(s => s.UUID))
			};
		}
	}
}