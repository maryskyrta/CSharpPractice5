using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CSharpPractice5.Models;
using CSharpPractice5.Tools;

namespace CSharpPractice5.ViewModels
{
    internal class ProcessListViewModel:INotifyPropertyChanged
    {

        #region Fields

        private ObservableCollection<SystemProcess> _processes;
        private RelayCommand<object> _terminateProcess;
        private Task _listRefreshTask;
        private Task _processesRefreshTask;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        public ObservableCollection<SystemProcess> Processes
        {
            get { return _processes; }
            set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> TerminateProcessCommand
        {
            get { return _terminateProcess ?? (_terminateProcess = new RelayCommand<object>(TerminateProcess, CanTerminateProcess)); }
        }

        public SystemProcess SelectedProcess { get; set; }

        

        #endregion


        #region Constructor

        
        public ProcessListViewModel()
        {
            ComputerHelper.Initialize();
            _processes = new ObservableCollection<SystemProcess>();
            var pros = Process.GetProcesses();
            foreach (var process in pros)
            {
                _processes.Add(new SystemProcess(process));
            }
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            StartBackgroundTasks();
            StationManager.StopThreads += StopBackgroundTasks;
        }

        #endregion


        #region Helping functions


        //private void FillProcesses(ObservableCollection<SystemProcess> processes)
        //{
        //    var pros = Process.GetProcesses();
        //    try
        //    {
        //        foreach (var process in pros)
        //        {

        //            processes.Add(new SystemProcess(process));
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.Message);
        //    }
        //}


        private void TerminateProcess(object obj)
        {
            MessageBox.Show($"{SelectedProcess.Name}");
            //TODO implement process termination
        }

        private bool CanTerminateProcess(object obj)
        {
            return SelectedProcess!=null;
        }

        private void StartBackgroundTasks()
        {
            _listRefreshTask = Task.Factory.StartNew(ListRefreshTaskProcess, TaskCreationOptions.LongRunning);
            _processesRefreshTask = Task.Factory.StartNew(ProcessesRefreshTaskProcess, TaskCreationOptions.LongRunning);
        }

        private void ListRefreshTaskProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                var pros = Process.GetProcesses();
                var processes = pros.Select(process => new SystemProcess(process)).ToList();
                Processes = new ObservableCollection<SystemProcess>(processes);
                //for (int j = 0; j < 3; j++)
                //{
                //    Thread.Sleep(500);
                //    if (_token.IsCancellationRequested)
                //        break;
                //}
                if (_token.IsCancellationRequested)
                    break;
                for (int j = 0; j < 10; j++)
                {
                    Thread.Sleep(500);
                    if (_token.IsCancellationRequested)
                        break;
                }
                if (_token.IsCancellationRequested)
                    break;
                i++;
            }
        }

        private void ProcessesRefreshTaskProcess()
        {
            int i = 0;
            while (!_token.IsCancellationRequested)
            {
                var pros = _processes.ToList();
                foreach (var process in pros)
                {
                    process.Refresh();
                }
                Processes = new ObservableCollection<SystemProcess>(pros);
                //for (int j = 0; j < 3; j++)
                //{
                //    Thread.Sleep(500);
                //    if (_token.IsCancellationRequested)
                //        break;
                //}
                if (_token.IsCancellationRequested)
                    break;
                for (int j = 0; j < 4; j++)
                {
                    Thread.Sleep(500);
                    if (_token.IsCancellationRequested)
                        break;
                }
                if (_token.IsCancellationRequested)
                    break;
                i++;
            }
        }

        internal void StopBackgroundTasks()
        {
            _tokenSource.Cancel();
            _listRefreshTask.Wait(2000);
            _listRefreshTask.Dispose();
            _processesRefreshTask.Wait(2000);
            _processesRefreshTask.Dispose();
            _listRefreshTask = null;
            _processesRefreshTask = null;
        }

        #endregion

        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

