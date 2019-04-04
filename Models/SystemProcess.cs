using System;
using System.Diagnostics;
using System.Management;
using System.Windows;

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
        private string _userName;
        private string _fileName;
        private string _filePath;
        private DateTime _startTime;

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
                try
                {
                    Process.GetProcessById(_id);
                }
                catch(Exception e)
                {
                    return "Not active";
                }
                return "Active";
                
            }
            
        }

        public double CpuPercent
        {
            get { return _cpuPercent; }
            set { _cpuPercent = value; }
        }

        public double MemoryPercent
        {
            get { return _memoryPercent; }
            set { _memoryPercent = value; }
        }

        public double MemoryVolume
        {
            get { return _memoryVolume; }
            set { _memoryVolume = value; }
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
        }

        #endregion

        private string GetOwner()
        {
            string query = "Select * From Win32_Process Where ProcessID = " + _id;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    return argList[0];
                }
            }

            return "NO OWNER";
        }
    }
    
}

