using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace bms.startup.command
{
    class gen2BalanceCommand : ICommand
    {
        public delegate void ICommandOnExecute(Object o);
        public delegate bool ICommandOnCanExecute(object parameter);
        private Object num;
        public gen2BalanceCommand(ICommandOnExecute onExecuteMethod, ICommandOnCanExecute onCanExecuteMethod, int i)
        {
            _execute = onExecuteMethod;
            _canExecute = onCanExecuteMethod;
            num = i;
        }

        private ICommandOnExecute _execute;
        private ICommandOnCanExecute _canExecute;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            // _execute.Invoke(parameter);
            _execute.Invoke((int)num);
        }
    }
}
