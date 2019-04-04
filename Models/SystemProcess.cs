using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows;
using CSharpPractice5.Tools;

namespace CSharpPractice5.Models
{
    internal class SystemProcess
    {
        #region Fields

        private Process _process;
        private string _name;
        private int _id;
        private double _cpuPercent;
        private double _memoryPercent;
        private double _memoryVolume;
        private readonly string _userName;
        private readonly string _fileName;
        private readonly string _filePath;
        private readonly DateTime _startTime;


        #endregion


        #region Properties

        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

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

        public double CpuPercent
        {
            get { return _cpuPercent; }
        }

        public double MemoryPercent
        {
            get { return _memoryPercent; }
        }

        public double MemoryVolume
        {
            get { return _memoryVolume; }
        }

        public int Threads
        {
            get { return _process.Threads.Count; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public string StartTime
        {
            get { return _startTime.ToString(); }
        }

        #endregion


        #region Constructor

        public SystemProcess(Process process)
        {
            _process = process;
            _name = _process.ProcessName;
            _id = _process.Id;
            _userName = _process.StartInfo.UserName;
            _filePath = _process.MainModule.FileName;
            _fileName = _filePath.Substring(_filePath.LastIndexOf('\\')+1);
            _startTime = _process.StartTime;
            _userName = GetOwner();
            Refresh();
        }

        #endregion

        private string GetOwner()
        {
            string query = "Select * From Win32_Process Where ProcessID = " + _id;
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
            PerformanceCounter memoryCounter = new PerformanceCounter("Process", "Private Bytes", _name, true);
            _memoryVolume = Convert.ToInt32(memoryCounter.NextValue()) / (int) (1024 * 1024);
            _memoryPercent = Math.Round((_memoryVolume / ComputerHelper.TotalRAM) * 100,2);
            PerformanceCounter cpuCounter  = new PerformanceCounter("Process", "% Processor Time", _name,true);
            _cpuPercent = Convert.ToInt32(cpuCounter.NextValue()/Environment.ProcessorCount);
            
        }
    }
    
}

