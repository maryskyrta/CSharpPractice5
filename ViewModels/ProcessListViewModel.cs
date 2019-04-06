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

        private ObservableCollection<SystemProcessModule> _processModules;
        private ObservableCollection<SystemProcessThread> _processThreads;
        private string _sortBy;
        private ObservableCollection<SystemProcess> _processes;
        private SystemProcess _selectedProcess;
        private RelayCommand<object> _terminateProcess;
        private RelayCommand<object> _openSourceFolder;
        private Task _listRefreshTask;
        private Task _processesRefreshTask;
        private readonly CancellationToken _token;
        private readonly CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        public SystemProcess SelectedProcess
        {
            get { return _selectedProcess;}
            set
            {
                _selectedProcess = value;
                DisplayThreads();
                DisplayModules();
                OnPropertyChanged();
            }
        }

        public static List<string> SortFields { get; } = new List<string> { "Name", "Id", "Active", "CPU%", "Memory%", "Memory usage", "Threads", "User", "File name", "File path", "Start time" };


        public ObservableCollection<SystemProcess> Processes
        {
            get { return _processes; }
            set
            {
                var id = -1;
                if (SelectedProcess != null)
                {
                    id = SelectedProcess.Id;
                }
                _processes = value;
                if (id > 0)
                {
                    var selected = new List<SystemProcess>(from process in _processes
                                                           where process.Id == id
                                                           select process);
                    var selItem = selected.Count == 0 ? null : selected.First();
                    SelectedProcess = selItem;
                }
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
            get { return _terminateProcess ?? (_terminateProcess = new RelayCommand<object>(TerminateProcess, CanProcessCommandExecute)); }
        }

        public RelayCommand<object> OpenSourceFolderCommand
        {
            get { return _openSourceFolder ?? (_openSourceFolder = new RelayCommand<object>(OpenSourceFolder, CanProcessCommandExecute)) ; }
        }

        public ObservableCollection<SystemProcessThread> ProcessThreads
        {
            get { return _processThreads; }
            set
            {
                _processThreads = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SystemProcessModule> ProcessModules
        {
            get { return _processModules; }
            set
            {
                _processModules = value; 
                OnPropertyChanged();
            }
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
            _processThreads = new ObservableCollection<SystemProcessThread>();
            _processModules = new ObservableCollection<SystemProcessModule>();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            StartBackgroundTasks();
            StationManager.StopThreads += StopBackgroundTasks;
        }

        #endregion


        #region Helping functions
        

        private void TerminateProcess(object obj)
        {
            try
            {
                _selectedProcess.Terminate();
                SelectedProcess = null;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OpenSourceFolder(object obj)
        {
            try
            {
                string path = _selectedProcess.FilePath.Substring(0, _selectedProcess.FilePath.LastIndexOf('\\'));
                Process.Start("explorer", path);
            }
            catch (Exception e)
            {
                MessageBox.Show("Source folder cannot be opened");
            }
        }

        private bool CanProcessCommandExecute(object obj)
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
                default:
                    return;
            }
        }

        private void DisplayThreads()
        {
            if (_selectedProcess != null||_selectedProcess.IsActive == "Not active")
            {
                var threads = new List<SystemProcessThread>();
                foreach (var thread in _selectedProcess.ProcessThreads())
                {
                    threads.Add(new SystemProcessThread((ProcessThread) thread));
                }
                ProcessThreads = new ObservableCollection<SystemProcessThread>(threads);
            }
            else ProcessThreads = new ObservableCollection<SystemProcessThread>();
        }

        private void DisplayModules()
        {
            if (_selectedProcess != null||_selectedProcess.IsActive=="Not active")
            {
                var modules = new List<SystemProcessModule>();
                foreach (var module in _selectedProcess.ProcessModules())
                {
                    modules.Add(new SystemProcessModule((ProcessModule)module));
                }
                ProcessModules = new ObservableCollection<SystemProcessModule>(modules);
            }
            else ProcessModules = new ObservableCollection<SystemProcessModule>();
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

