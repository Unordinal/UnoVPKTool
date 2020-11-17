using System;
using System.Windows.Input;

namespace UnoVPKTool.WPF.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
        {
            if (execute is null) throw new ArgumentNullException(nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<T> execute) : this(execute, null) { }

        public virtual bool CanExecute(T parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public virtual void Execute(T parameter)
        {
            _execute(parameter);
        }

        bool ICommand.CanExecute(object? parameter)
        {
            return CanExecute((T)parameter!);
        }

        void ICommand.Execute(object? parameter)
        {
            Execute((T)parameter!);
        }
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Func<bool>? canExecute) : base((_) => execute(), (canExecute is not null) ? (_) => canExecute.Invoke() : null) { }

        public RelayCommand(Action execute) : this(execute, null) { }
    }
}