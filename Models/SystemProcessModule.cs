using System.Diagnostics;

namespace CSharpPractice5.Models
{
    internal class SystemProcessModule
    {
        public string Name { get; }
        public string Path { get; }

        public SystemProcessModule(ProcessModule module)
        {
            Name = module.ModuleName;
            Path = module.FileName;
        }
    }
}
