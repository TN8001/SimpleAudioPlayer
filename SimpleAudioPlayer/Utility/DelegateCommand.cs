using System;
using System.Windows.Input;

namespace SimpleAudioPlayer
{
    public sealed class DelegateCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public DelegateCommand(Action execute) : this(execute, () => true) { }
        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute() => _canExecute();
        public void Execute() => _execute();
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        bool ICommand.CanExecute(object parameter) => CanExecute();
        void ICommand.Execute(object parameter) => Execute();
    }

    public sealed class DelegateCommand<T> : ICommand
    {
        private Action<T> _execute;
        private Func<T, bool> _canExecute;
        private static readonly bool IS_VALUE_TYPE;

        static DelegateCommand()
        {
            IS_VALUE_TYPE = typeof(T).IsValueType;
        }

        public DelegateCommand(Action<T> execute) : this(execute, o => true) { }
        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(T parameter) => _canExecute(parameter);
        public void Execute(T parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        bool ICommand.CanExecute(object parameter) => CanExecute(Cast(parameter));
        void ICommand.Execute(object parameter) => Execute(Cast(parameter));
        private T Cast(object parameter)
        {
            if(parameter == null && IS_VALUE_TYPE)
                return default(T);
            return (T)parameter;
        }
    }
}
