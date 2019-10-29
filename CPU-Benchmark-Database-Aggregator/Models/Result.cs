#region using

using System;
using System.Collections.Generic;
using HardwareInformation;
using Newtonsoft.Json;

#endregion

namespace CPU_Benchmark_Server_Aggregator.Models
{
    public class Result
    {
        public Result(string benchmark, double timing, double points, double referenceTiming, double referencePoints)
        {
            Benchmark = benchmark;
            Timing = timing;
            Points = points;
            ReferenceTiming = referenceTiming;
            ReferencePoints = referencePoints;
        }

        public Result()
        {
            // Stupid JSON
        }

        public string Benchmark { get; set; }
        public double Points { get; set; }

        [JsonIgnore] public double ReferencePoints { get; set; }

        [JsonIgnore] public double ReferenceTiming { get; set; }

        public double Timing { get; set; }
    }

    public class Save
    {
        public string DotNetVersion;
        public MachineInformation MachineInformation;
        public Dictionary<uint, List<Result>> Results;
        public string UUID;
        public Version Version;

        public Save()
        {
            Results = new Dictionary<uint, List<Result>>();
        }
    }
}