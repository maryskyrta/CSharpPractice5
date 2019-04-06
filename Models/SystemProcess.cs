using System;
using System.ComponentModel;
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
                try
                {
                    Process.GetProcessById(Id);
                }
                catch (Exception e)
                {
                    return "Not active";
                }
                return "Active";

            }
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

            try
            {
                FilePath = _process.MainModule.FileName;
                StartTime = _process.StartTime.ToString();
            }
            catch (Exception ex)
            {
                FilePath = "Undefined";
                StartTime = "Undefined";
            }
            FileName = FilePath.Substring(FilePath.LastIndexOf('\\')+1);
            UserName = GetOwner();
            Threads = _process.Threads.Count;
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
            if (obj != null)
            {
                string[] argList = new string[] {string.Empty, string.Empty};
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    return argList[0];
                }
            }

            return "Undefined";
        }

        public void Refresh()
        {
            try
            {
                MemoryVolume = Convert.ToInt32(_memoryCounter.NextValue()) / (int) (1024 * 1024);
                MemoryPercent = Math.Round((MemoryVolume / ComputerHelper.TotalRAM) * 100, 2);
                CpuPercent = Math.Round(_cpuCounter.NextValue() / Environment.ProcessorCount,2);
            }
            catch(System.InvalidOperationException)
            {

            }
        }

        public void Terminate()
        {
            if(IsActive=="Active")
                _process.Kill();
        }

        public ProcessThreadCollection ProcessThreads()
        {
            return _process.Threads;
        }

        public ProcessModuleCollection ProcessModules()
        {
            try
            {
                return _process.Modules;
            }
            catch (Win32Exception)
            {
                return null;
            }
        }

        #endregion
    }

}

