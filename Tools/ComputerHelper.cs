using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSharpPractice5.Tools
{
    internal static class ComputerHelper
    {
        public static double TotalRAM;

        internal static void Initialize()
        {
            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            ManagementObjectCollection results = searcher.Get();
            foreach (ManagementObject result in results)
            {
                double res = Convert.ToDouble(result["TotalVisibleMemorySize"]);
                double fres = Math.Round((res / 1024), 2);
                TotalRAM = fres;
                MessageBox.Show($"{fres}");
            }
        }
    }
}
