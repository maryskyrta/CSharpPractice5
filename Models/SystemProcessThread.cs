using System;
using System.Diagnostics;
using System.Windows;

namespace CSharpPractice5.Models
{
    internal class SystemProcessThread
    {
        
        public int Id { get;  }
        public string State { get;  }
        public string StartTime { get;  }


        public SystemProcessThread(ProcessThread thread)
        {
            try
            {
                Id = thread.Id;
                State = thread.ThreadState.ToString();
                StartTime = thread.StartTime.ToString();
            }
            catch(Exception e)
            {
                StartTime = null;
            }
        }
    }
}
