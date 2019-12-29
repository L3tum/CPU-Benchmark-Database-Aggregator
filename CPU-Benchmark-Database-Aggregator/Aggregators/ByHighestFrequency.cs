#region using

using System.Collections.Generic;
using CPU_Benchmark_Common;
using CPU_Benchmark_Database_Aggregator.Models;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class ByHighestFrequency : IAggregator
	{
		private readonly List<Entry> singleCoreHighest = new List<Entry>();

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
				    uint.Parse(singleCoreHighest[i].Value.Split(" === ")[2]))
				{
					singleCoreHighest.Insert(i,
						new Entry
						{
							SaveFile = save.UUID,
							Value =
								$"{save.MachineInformation.Cpu.Name} === {save.MachineInformation.Cpu.Vendor} === {save.MachineInformation.Cpu.MaxClockSpeed.ToString()}"
						});
					inserted = true;
					break;
				}
			}

			if (!inserted && singleCoreHighest.Count < 100)
			{
				singleCoreHighest.Add(new Entry
				{
					SaveFile = save.UUID,
					Value =
						$"{save.MachineInformation.Cpu.Name} === {save.MachineInformation.Cpu.Vendor} === {save.MachineInformation.Cpu.MaxClockSpeed.ToString()}"
				});
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			return new List<Aggregate>
			{
				new Aggregate("single-core", "byHighestFrequency", singleCoreHighest)
			};
		}
	}
}