﻿using Prism.Events;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkyPC_AutoMusic.View
{
    /// <summary>
    /// UserControlList.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlList : UserControl
    {
        public UserControlList()
        {
            InitializeComponent();
            EA.EventAggregator.GetEvent<EnableListEvent>().Subscribe(value => { IsEnabled = value; });
            DataContext = new ListViewModel();
        }

    }
}
