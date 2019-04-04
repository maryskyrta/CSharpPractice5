using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CSharpPractice5.Models;

namespace CSharpPractice5.ViewModels
{
    internal class ProcessListViewModel
    {
        #region Fields

        
        private ObservableCollection<SystemProcess> _processes;


        #endregion


        public ObservableCollection<SystemProcess> Processes
        {
            get { return _processes; }
            set { _processes = value; }
        }

        #region Constructor

        
        public ProcessListViewModel()
        {
            _processes = new ObservableCollection<SystemProcess>();
            FillProcesses();
        }

        #endregion


        private void FillProcesses()
        {
            var pros = Process.GetProcesses();
            try
            {
                foreach (var process in pros)
                {

                    _processes.Add(new SystemProcess(process));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        }
    }

