using SkyPC_AutoMusic.Command;
using SkyPC_AutoMusic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class TitleBarNormalViewModel : NotificationObject
    {
        public DelegateCommand WindowMinimizeCommand { get; set; }

        public DelegateCommand WindowCloseCommand { get; set; }

        public DelegateCommand WindowShrinkCommand { get; set; }

        public TitleBarNormalViewModel()
        {
            //命令
            WindowMinimizeCommand = new DelegateCommand(WindowMinimize);
            WindowCloseCommand = new DelegateCommand(WindowClose);
            WindowShrinkCommand = new DelegateCommand(WindowShrink);
        }

        private void WindowMinimize()
        {
            EA.EventAggregator.GetEvent<WindowMinimizeEvent>().Publish();
        }

        private void WindowClose()
        {
            EA.EventAggregator.GetEvent<WindowCloseEvent>().Publish();
        }

        private void WindowShrink()
        {
            EA.EventAggregator.GetEvent<WindowExpandEvent>().Publish(false);
        }
    }
}
