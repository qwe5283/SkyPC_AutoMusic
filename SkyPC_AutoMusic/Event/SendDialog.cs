using MaterialDesignThemes.Wpf;
using SkyPC_AutoMusic.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SkyPC_AutoMusic.Event
{
    public class SendDialog
    {
        public static void MessageTips(string message)
        {
            var messageDialog = new UserControlMessageDialog
            {
                Message = { Text = message }
            };
            DialogHost.Show(messageDialog, "RootDialog");
        }

        public static object WaitTips(string message,int total)
        {
            var waitDialog = new UserControlWaitDialog(total)
            {
                Message = { Content = message },
                Total = {Content = total},
            };
            DialogHost.Show(waitDialog, "RootDialog");
            return waitDialog;
        }
    }
}
