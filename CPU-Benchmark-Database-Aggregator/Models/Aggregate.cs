using CPU_Benchmark_Server_Aggregator.Models;
using HardwareInformation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPU_Benchmark_Database_Aggregator.Models
{
    class Aggregate
    {
        public List<Result> ResultsST = new List<Result>();
        public List<Result> ResultsMT = new List<Result>();
        public MachineInformation MachineInformation = new MachineInformation();
    }
}
