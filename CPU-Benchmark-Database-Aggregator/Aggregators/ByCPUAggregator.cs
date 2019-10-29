using CPU_Benchmark_Database_Aggregator.Models;
using CPU_Benchmark_Server_Aggregator.Models;
using HardwareInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPU_Benchmark_Database_Aggregator.Aggregators
{
    class ByCPUAggregator : IAggregator
    {
        private Dictionary<string, Dictionary<string, List<Result>>> ResultsST = new Dictionary<string, Dictionary<string, List<Result>>>();
        private Dictionary<string, Dictionary<string, List<Result>>> ResultsMT = new Dictionary<string, Dictionary<string, List<Result>>>();
        private Dictionary<string, MachineInformation> MetaInfos = new Dictionary<string, MachineInformation>();

        public IEnumerable<Aggregate> GetAggregatedResults()
        {
            var aggregates = new List<Aggregate>();

            foreach(var keyValuePair in MetaInfos)
            {
                var aggregate = new Aggregate() { MachineInformation = keyValuePair.Value };

                foreach(var dic in ResultsST[keyValuePair.Value.Cpu.Caption])
                {
                    aggregate.ResultsST.Add(new Result() { Benchmark = dic.Key, Points = dic.Value.Average(r => r.Points), Timing = dic.Value.Average(r => r.Timing) });
                }

                foreach (var dic in ResultsMT[keyValuePair.Value.Cpu.Caption])
                {
                    aggregate.ResultsMT.Add(new Result() { Benchmark = dic.Key, Points = dic.Value.Average(r => r.Points), Timing = dic.Value.Average(r => r.Timing) });
                }

                aggregates.Add(aggregate);
            }

            return aggregates;
        }

        public void ProcessSave(Save save)
        {
            if (!ResultsST.ContainsKey(save.MachineInformation.Cpu.Caption))
            {
                ResultsST.Add(save.MachineInformation.Cpu.Caption, new Dictionary<string, List<Result>>());
            }

            if (!ResultsMT.ContainsKey(save.MachineInformation.Cpu.Caption))
            {
                ResultsMT.Add(save.MachineInformation.Cpu.Caption, new Dictionary<string, List<Result>>());
            }

            if (!MetaInfos.ContainsKey(save.MachineInformation.Cpu.Caption))
            {
                MetaInfos.Add(save.MachineInformation.Cpu.Caption, new MachineInformation() { Cpu = save.MachineInformation.Cpu });

                MetaInfos[save.MachineInformation.Cpu.Caption].Cpu.Cores.Clear();
            }

            foreach (var coreConfig in save.Results)
            {
                var dictionary = coreConfig.Key == 1 ? ResultsST[save.MachineInformation.Cpu.Caption] : 
                    coreConfig.Key == save.MachineInformation.Cpu.LogicalCores ? ResultsMT[save.MachineInformation.Cpu.Caption] : null;

                if(dictionary == null)
                {
                    continue;
                }

                foreach(var result in coreConfig.Value)
                {
                    if (!dictionary.ContainsKey(result.Benchmark))
                    {
                        dictionary.Add(result.Benchmark, new List<Result>());
                    }

                    dictionary[result.Benchmark].Add(result);
                }
            }
        }
    }
}
