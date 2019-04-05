using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPractice5.Tools
{
    internal static class StationManager
    {
        public static event Action StopThreads;

        internal static void CloseApp()
        {
            StopThreads?.Invoke();
            Environment.Exit(1);
        }
    }
}
