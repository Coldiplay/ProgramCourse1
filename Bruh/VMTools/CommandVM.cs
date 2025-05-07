using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bruh.VMTools
{
    public class CommandVM : ICommand
    {
        public event EventHandler? CanExecuteChanged
        { 
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        Action action;
        Func<bool> canExecute;

        public CommandVM(Action action, Func<bool> func)
        {
            this.action = action;
            canExecute = func;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute();
        }

        public void Execute(object? parameter)
        {
            action();
        }
    }
}
