#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CPU_Benchmark_Database_Aggregator.Aggregators;
using CPU_Benchmark_Database_Aggregator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace CPU_Benchmark_Database_Aggregator
{
	internal class Program
	{
		private static readonly IEnumerable<IAggregator> aggregators = new IAggregator[]
		{
			new PaginationAggregator(),
			new ByHighestFrequency(),
			new AverageByCpuAggregator()
		};

		internal static string SAVES_DIRECTORY { get; private set; }
		internal static string AGGREGATIONS_DIRECTORY { get; private set; }

		private static void Main(string[] args)
		{
			SAVES_DIRECTORY = (Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? ".") + "/saves";
			AGGREGATIONS_DIRECTORY = (Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? ".") + "/aggregations";

			var saves = Directory.GetFiles(SAVES_DIRECTORY, "*.json");
			var aggregates = new List<Aggregate>();
			var settings = new JsonSerializerSettings();

			settings.Converters.Add(new StringEnumConverter());

			foreach (var saveFile in saves)
			{
				// Skip automatically generated saves
				if (saveFile.Contains(".automated."))
				{
					continue;
				}

				Console.Write('.');

				try
				{
					Save save;

					try
					{
						save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(saveFile), settings);
					}
					catch (Exception)
					{
						Console.WriteLine();
						Console.WriteLine(saveFile);
						Console.WriteLine("Retrying...");
						save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(saveFile));
						Console.WriteLine();
					}

					save.UUID = saveFile.Split('/').Last().Replace(".json", "");

					foreach (var aggregator in aggregators)
					{
						try
						{
							aggregator.ProcessSave(save);
						}
						catch (Exception e)
						{
							Console.WriteLine();
							Console.WriteLine(
								$"Aggregator {aggregator.GetType().Name} failed processing save {saveFile}!");
							Console.WriteLine(e);
							Console.WriteLine();
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine(saveFile);
					Console.WriteLine(e);
					Console.WriteLine();
				}
			}

			foreach (var aggregator in aggregators)
			{
				try
				{
					aggregates.AddRange(aggregator.GetAggregatedResults());
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine($"Aggregator {aggregator.GetType().Name} failed getting results!");
					Console.WriteLine(e);
					Console.WriteLine();
				}
			}

			if (Directory.Exists(AGGREGATIONS_DIRECTORY))
			{
				Directory.Delete(AGGREGATIONS_DIRECTORY, true);
			}

			Directory.CreateDirectory(AGGREGATIONS_DIRECTORY);

			foreach (var aggregate in aggregates)
			{
				var dir = $"{AGGREGATIONS_DIRECTORY}/{aggregate.Category}";

				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}

				var validName = new StringBuilder(aggregate.Name);

				foreach (var c in Path.GetInvalidFileNameChars())
				{
					validName = validName.Replace(c, '_');
				}

				validName = validName.Replace("@", "at").Replace(" ", "_").Replace(":", "_");

				var file = $"{dir}/{validName}.json";

				if (File.Exists(file))
				{
					File.Delete(file);
				}

				File.WriteAllText(file, JsonConvert.SerializeObject(aggregate));
			}
		}
	}
}