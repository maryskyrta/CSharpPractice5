using System;

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
