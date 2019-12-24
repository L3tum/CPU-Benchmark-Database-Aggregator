#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CPU_Benchmark_Database_Aggregator.Models;
using HardwareInformation;
using Newtonsoft.Json;

#endregion

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
	internal class AverageByCpuAggregator : IAggregator
	{
		private readonly Dictionary<string, List<Save>> byCpu = new Dictionary<string, List<Save>>();
		private readonly Random random = new Random();

		public void ProcessSave(Save save)
		{
			var cpu = save.MachineInformation.Cpu.Caption;

			if (!byCpu.ContainsKey(cpu))
			{
				byCpu.Add(cpu, new List<Save> {save});

				return;
			}

			if (byCpu[cpu].Count >= 100)
			{
				var index = random.Next(0, byCpu[cpu].Count);

				byCpu[cpu][index] = save;
			}
			else
			{
				byCpu[cpu].Add(save);
			}
		}

		public IEnumerable<Aggregate> GetAggregatedResults()
		{
			var saveFiles = new List<Tuple<string, string>>();

			foreach (var pair in byCpu)
			{
				var save = new Save();

				save.MachineInformation = new MachineInformation();
				save.MachineInformation.RAMSticks = new List<MachineInformation.RAM>();
				save.MachineInformation.Cpu = new MachineInformation.CPU();
				save.MachineInformation.SmBios = new MachineInformation.SMBios();
				save.MachineInformation.Cpu = pair.Value[0].MachineInformation.Cpu;
				save.MachineInformation.Cpu.Cores.Clear();
				save.MachineInformation.Cpu.MaxClockSpeed =
					(uint) Math.Round(pair.Value.Average(s => s.MachineInformation.Cpu.MaxClockSpeed), 0);
				save.MachineInformation.Cpu.PhysicalCores =
					(uint) Math.Round(pair.Value.Average(s => s.MachineInformation.Cpu.PhysicalCores), 0);
				save.MachineInformation.Cpu.LogicalCores =
					(uint) Math.Round(pair.Value.Average(s => s.MachineInformation.Cpu.LogicalCores), 0);
				save.MachineInformation.Cpu.LogicalCoresPerNode =
					(uint) Math.Round(pair.Value.Average(s => s.MachineInformation.Cpu.LogicalCoresPerNode), 0);
				save.MachineInformation.OperatingSystem = pair.Value.GroupBy(s => s.MachineInformation.OperatingSystem)
					.OrderByDescending(gp => gp.Count()).First().Key;
				save.MachineInformation.Platform = MachineInformation.Platforms.Windows;

				var ramSticks = Math.Round(pair.Value.Average(s => s.MachineInformation.RAMSticks.Count), 0);

				for (var i = 0; i < ramSticks; i++)
				{
					var capacity =
						(ulong) Math.Round(
							pair.Value.Average(s =>
								(decimal) (s.MachineInformation.RAMSticks.Count > i
									? s.MachineInformation.RAMSticks[i].Capacity
									: s.MachineInformation.RAMSticks.First().Capacity)),
							0);
					var speed =
						(uint) Math.Round(
							pair.Value.Average(s =>
								(decimal) (s.MachineInformation.RAMSticks.Count > i
									? s.MachineInformation.RAMSticks[i].Speed
									: s.MachineInformation.RAMSticks.First().Speed)),
							0);

					var manufacturer = pair.Value.GroupBy(s => s.MachineInformation.RAMSticks.Count > i
							? s.MachineInformation.RAMSticks[i].Manfucturer
							: s.MachineInformation.RAMSticks.First().Manfucturer)
						.OrderByDescending(gp => gp.Count()).First().Key;

					var formFactor = pair.Value.GroupBy(s => s.MachineInformation.RAMSticks.Count > i
							? s.MachineInformation.RAMSticks[i].FormFactor
							: s.MachineInformation.RAMSticks.First().FormFactor)
						.OrderByDescending(gp => gp.Count()).First().Key;

					save.MachineInformation.RAMSticks.Add(new MachineInformation.RAM
					{
						Capacity = capacity,
						Speed = speed,
						CapacityHRF = FormatBytes(capacity),
						Manfucturer = manufacturer,
						FormFactor = formFactor
					});
				}

				save.MachineInformation.SmBios.BIOSCodename = pair.Value
					.GroupBy(s => s.MachineInformation.SmBios.BIOSCodename).OrderByDescending(gp => gp.Count()).First()
					.Key;
				save.MachineInformation.SmBios.BIOSVendor = pair.Value
					.GroupBy(s => s.MachineInformation.SmBios.BIOSVendor).OrderByDescending(gp => gp.Count()).First()
					.Key;
				save.MachineInformation.SmBios.BIOSVersion = pair.Value
					.GroupBy(s => s.MachineInformation.SmBios.BIOSVersion).OrderByDescending(gp => gp.Count()).First()
					.Key;
				save.MachineInformation.SmBios.BoardName = pair.Value
					.GroupBy(s => s.MachineInformation.SmBios.BoardName).OrderByDescending(gp => gp.Count()).First()
					.Key;
				save.MachineInformation.SmBios.BoardVendor = pair.Value
					.GroupBy(s => s.MachineInformation.SmBios.BoardVendor).OrderByDescending(gp => gp.Count()).First()
					.Key;
				save.MachineInformation.SmBios.BoardVersion = pair.Value
					.GroupBy(s => s.MachineInformation.SmBios.BoardVersion).OrderByDescending(gp => gp.Count()).First()
					.Key;

				save.DotNetVersion = pair.Value.GroupBy(s => s.DotNetVersion).OrderByDescending(gp => gp.Count())
					.First().Key;

				save.Version = pair.Value.GroupBy(s => s.Version).OrderByDescending(gp => gp.Count()).First().Key;
				save.UUID = $"AUTOMATED-AVERAGE-{save.MachineInformation.Cpu.Caption}";

				for (uint i = 1; i <= save.MachineInformation.Cpu.LogicalCores; i++)
				{
					var benchmarks = new List<string>();
					var perCoresResults = pair.Value.Where(s => s.Results.ContainsKey(i)).Select(s => s.Results[i])
						.ToList();

					foreach (var results in perCoresResults)
					{
						foreach (var result in results)
						{
							if (!benchmarks.Contains(result.Benchmark))
							{
								benchmarks.Add(result.Benchmark);
							}
						}
					}

					foreach (var benchmark in benchmarks)
					{
						var perBenchmarkPoints = 0.0;
						var perBenchmarkTiming = 0.0;
						var refPerBenchmarkPoints = 0.0;
						var refPerBenchmarkTiming = 0.0;
						var count = 0;

						foreach (var perCoresResult in perCoresResults)
						{
							foreach (var result in perCoresResult.Where(result => result.Benchmark == benchmark))
							{
								perBenchmarkPoints += result.Points;
								perBenchmarkTiming += result.Timing;
								refPerBenchmarkPoints += result.ReferencePoints;
								refPerBenchmarkTiming += result.ReferenceTiming;

								count++;
							}
						}

						perBenchmarkPoints /= count;
						perBenchmarkTiming /= count;
						refPerBenchmarkPoints /= count;
						refPerBenchmarkTiming /= count;

						if (!save.Results.ContainsKey(i))
						{
							save.Results.Add(i, new List<Result>());
						}

						save.Results[i].Add(new Result(benchmark, perBenchmarkTiming, perBenchmarkPoints,
							refPerBenchmarkTiming, refPerBenchmarkPoints));
					}
				}

				var fileName = save.MachineInformation.Cpu.Caption.Replace("@", "at").Replace(" ", "_")
					.Replace(",", "_");

				var file = $"{Program.SAVES_DIRECTORY}/average_{fileName}.automated.json";

				if (File.Exists(file))
				{
					File.Delete(file);
				}

				File.WriteAllText(file, JsonConvert.SerializeObject(save));

				saveFiles.Add(Tuple.Create($"average_{fileName}.automated", save.MachineInformation.Cpu.Caption));
			}

			return new[]
			{
				new Aggregate("average", "byCPU",
					saveFiles.Select(tuple => new Entry {SaveFile = tuple.Item1, Value = tuple.Item2}))
			};
		}

		private string FormatBytes(ulong bytes)
		{
			string[] Suffix = {"B", "KiB", "MiB", "GiB", "TiB", "PiB"};
			int i;
			double dblSByte = bytes;
			for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
			{
				dblSByte = bytes / 1024.0;
			}

			return string.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
		}
	}
}