using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using CSharpPractice5.Tools;

namespace CSharpPractice5.Models
{
    internal class SystemProcess
    {
        #region Fields

        private readonly Process _process;
        private readonly PerformanceCounter _memoryCounter;
        private readonly PerformanceCounter _cpuCounter;


        #endregion


        #region Properties


        public string Name { get; }

        public int Id { get; }

        public string IsActive
        {
            get
            {
                if (_process.Responding)
                    return "Responding";
                return "Not responding";
            }
            //get
            //{
            //    try
            //    {
            //        Process.GetProcessById(_id);
            //    }
            //    catch(Exception e)
            //    {
            //        return "Not active";
            //    }
            //    return "Active";

            //}
    }

        public double CpuPercent { get; private set; }

        public double MemoryPercent { get; private set; }

        public double MemoryVolume { get; private set; }

        public int Threads { get; private set; }

        public string UserName { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public string StartTime{ get; }
        

        #endregion


        #region Constructor

        public SystemProcess(Process process)
        {
            _process = process;
            Name = _process.ProcessName;
            Id = _process.Id;
            UserName = _process.StartInfo.UserName;
            FilePath = _process.MainModule.FileName;
            FileName = FilePath.Substring(FilePath.LastIndexOf('\\')+1);
            StartTime = _process.StartTime.ToString();
            UserName = GetOwner();
            _memoryCounter = new PerformanceCounter("Process", "Private Bytes", Name, true);
            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", Name, true);
            Refresh();
        }

        #endregion


        #region Helping functions

        
        private string GetOwner()
        {
            string query = "Select * From Win32_Process Where ProcessID = " + Id;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();
            ManagementObject obj = processList.OfType<ManagementObject>().FirstOrDefault();
            string[] argList = new string[] {string.Empty, string.Empty};
            int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
            if (returnVal == 0)
            {
                return argList[0];
            }
            return "Not defined";
        }

        private void Refresh()
        {
            MemoryVolume = Convert.ToInt32(_memoryCounter.NextValue()) / (int) (1024 * 1024);
            MemoryPercent = Math.Round((MemoryVolume / ComputerHelper.TotalRAM) * 100,2);
            CpuPercent = Convert.ToInt32(_cpuCounter.NextValue()/Environment.ProcessorCount);
            Threads = _process.Threads.Count;
        }
        
        #endregion
    }

}

