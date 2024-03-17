using System;
using System.Windows.Input;

namespace FlipbookMaker.Backend
{
    class BaseCommand
        : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private Action<object?> _execute;
        private Func<object?, bool> _canExecute;

        public BaseCommand(Action callback)
            : this((o) => callback.Invoke(), (o) => true)
        {
        }

        public BaseCommand(Action<object?> callback)
            : this(callback, (o) => true)
        {
        }

        public BaseCommand(Action<object?> execute, Func<object?, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public BaseCommand(Action callback, Func<object?, bool> canExecute)
           : this((o) => callback.Invoke(), canExecute)
        {
        }


        public bool CanExecute(object? parameter) => _canExecute.Invoke(parameter);
        public void Execute(object? parameter) => _execute.Invoke(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
