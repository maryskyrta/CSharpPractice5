using System;
using System.Windows.Input;

namespace CSharpPractice5.Tools
{
    internal class RelayCommand<T> : ICommand
    {

        #region Fields
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;
        #endregion

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {

        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion
    }
}
