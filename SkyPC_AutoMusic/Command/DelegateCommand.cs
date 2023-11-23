using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SkyPC_AutoMusic.Command
{
    internal class DelegateCommand : ICommand
    {
        Action executeAction;

        public DelegateCommand(Action action)
        {
            executeAction = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            executeAction();
        }
    }
}
