using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace bms.startup.command
{
    class balanceAllSelectCommand : ICommand
    {
        public delegate void ICommandOnExecute(CheckBox parameter,int i);
        public delegate bool ICommandOnCanExecute(object parameter);
        private Object checkbox;
        private Object slaveNum;
        public balanceAllSelectCommand(ICommandOnExecute onExecuteMethod, ICommandOnCanExecute onCanExecuteMethod, CheckBox cb,int i)
        {
            _execute = onExecuteMethod;
            _canExecute = onCanExecuteMethod;
            checkbox = cb;
            slaveNum = i;
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
            _execute.Invoke((CheckBox)checkbox,(int)slaveNum);
        }
    }
}
