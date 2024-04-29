using MaterialDesignThemes.Wpf;
using SkyPC_AutoMusic.Command;
using SkyPC_AutoMusic.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class FilterDialogViewModel:NotificationObject
    {
        private TextBox textBox;

        public DelegateCommand SetFilterCommand { get; set; }

        public FilterDialogViewModel(TextBox textBox,string text)
        {
            textBox.Text = text;
            this.textBox = textBox;
            SetFilterCommand = new DelegateCommand(SetFilter);
        }

        private void SetFilter()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
            EA.EventAggregator.GetEvent<SetFilterEvent>().Publish(textBox.Text);
        }
    }
}
