using MaterialDesignThemes.Wpf;
using SkyPC_AutoMusic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class WaitDialogViewModel:NotificationObject
    {
        private delegate void CloseDelegate();

        private int progress;

        private IInputElement dialog;

        public int Progress
        {
            get { return progress; }
            set 
            { 
                progress = value;
                if(IsTaskComplete())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DialogHost.CloseDialogCommand.Execute(null, dialog);
                    });
                }
                OnPropertyChanged();
            }
        }

        private int total;

        public int Total
        {
            get { return total; }
            set { total = value; OnPropertyChanged(); }
        }


        public WaitDialogViewModel(int total,IInputElement dialog)
        {
            this.dialog = dialog;
            Total = total;
            progress = 0;
            EA.EventAggregator.GetEvent<PassProgressToWaitDialogEvent>().Subscribe((num) => { Progress = num; });
        }

        private bool IsTaskComplete()
        {
            if (progress == 0 || total == 0)
                return false;

            return progress == total;
        }
    }
}
