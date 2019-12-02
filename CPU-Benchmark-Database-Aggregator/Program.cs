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
		private static readonly IEnumerable<IAggregator> aggregators = new[]
		{
			new ByScoreAggregator()
		};

		private static void Main(string[] args)
		{
			var baseDirectory = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") + "/saves";
			var saves = Directory.GetFiles(baseDirectory, "*.json");
			var aggregates = new List<Aggregate>();
			var settings = new JsonSerializerSettings();

			settings.Converters.Add(new StringEnumConverter());

			foreach (var saveFile in saves)
			{
				Console.Write('.');

				try
				{
					Save save;

					try
					{
						save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(saveFile), settings);
					}
					catch (Exception e)
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
						aggregator.ProcessSave(save);
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
				aggregates.AddRange(aggregator.GetAggregatedResults());
			}

			baseDirectory = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") + "/aggregations";

			foreach (var aggregate in aggregates)
			{
				var dir = $"{baseDirectory}/{aggregate.Category}";

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

				var file = $"{dir}/{validName}";

				if (File.Exists(file))
				{
					File.Delete(file);
				}

				File.WriteAllText(file, JsonConvert.SerializeObject(aggregate));
			}
		}
	}
}