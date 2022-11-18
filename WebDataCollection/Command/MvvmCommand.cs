using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WebDataCollection.Command
{
    public class MvvmCommand : ICommand
    {
        private readonly Action action;

        public event EventHandler CanExecuteChanged;


        public MvvmCommand(Action action)
        {
            this.action = action;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action?.Invoke();
        }
    }    
    public class MvvmCommand<T> : ICommand
    {
        private readonly Action<T> action;
        private readonly Func<T, bool> canExcute;

        public event EventHandler CanExecuteChanged;


        public MvvmCommand(Action<T> action)
        {
            this.action = action;
        }
        public MvvmCommand(Action<T> action,Func<T,bool> canExcute)
        {
            this.action = action;
            this.canExcute = canExcute;
        }
        public bool CanExecute(object parameter)
        {
            return canExcute is null ? true : canExcute.Invoke((T)parameter);
        }

        public void Execute(object parameter)
        {
            action?.Invoke((T)parameter);
        }
    }
}
