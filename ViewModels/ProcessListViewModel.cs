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

        private string _sortBy;
        private ObservableCollection<SystemProcess> _processes;
        private RelayCommand<object> _terminateProcess;
        private Task _listRefreshTask;
        private Task _processesRefreshTask;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        public SystemProcess SelectedProcess { get; set; }

        public static List<string> SortFields { get; } = new List<string> { "Name", "Id", "Active", "CPU%", "Memory%", "Memory usage", "Threads", "User", "File name", "File path", "Start time", "No sort" };


        public ObservableCollection<SystemProcess> Processes
        {
            get { return _processes; }
            set
            {
                _processes = value;
                OnPropertyChanged();
            }
        }

        public string SortBy
        {
            get { return _sortBy; }
            set
            {
                _sortBy = value;
                var sorted = _processes.ToList();
                SortProcesses(ref sorted);
                Processes = new ObservableCollection<SystemProcess>(sorted);
            }
        }

        public RelayCommand<object> TerminateProcessCommand
        {
            get { return _terminateProcess ?? (_terminateProcess = new RelayCommand<object>(TerminateProcess, CanTerminateProcess)); }
        }

        
        

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
                SortProcesses(ref processes);
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
                SortProcesses(ref pros);
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
            _listRefreshTask.Wait(5000);
            _listRefreshTask.Dispose();
            _processesRefreshTask.Wait(2000);
            _processesRefreshTask.Dispose();
            _listRefreshTask = null;
            _processesRefreshTask = null;
        }

        private void SortProcesses(ref List<SystemProcess> processes)
        {
            switch (_sortBy)
            {
                case "Name":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.Name
                        select process);
                    break;
                case "Id":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.Id
                        select process);
                    break;
                case "Active":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.IsActive
                        select process);
                    break;
                case "CPU%":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.CpuPercent
                        select process);
                    break;
                case "Memory%":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.MemoryPercent
                        select process);
                    break;
                case "Memory usage":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.MemoryVolume
                        select process);
                    break;
                case "Threads":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.Threads
                        select process);
                    break;
                case "User":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.UserName
                        select process);
                    break;
                case "File name":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.FileName
                        select process);
                    break;
                case "File path":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.FilePath
                        select process);
                    break;
                case "Start time":
                    processes = new List<SystemProcess>(from process in processes
                        orderby process.StartTime
                        select process);
                    break;
            }

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

