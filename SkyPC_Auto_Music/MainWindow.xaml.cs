using SkyPC_Auto_Music.ViewModel;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Interop;
using System.Net.NetworkInformation;

namespace SkyPC_Auto_Music
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserControl_Play pagePlay;
        public UserControl_Sheet pageSheet;

        public MainWindow()
        {
            InitializeComponent();

            SourceInitialized += OnSourceInitialized;

            pagePlay = new UserControl_Play(this);
            pageSheet = new UserControl_Sheet();

            StackPanelMain.Children.Add(pagePlay);
            
            string imgPath = AppDomain.CurrentDomain.BaseDirectory + "bg.jpg";

            try
            {
                BitmapImage img = new BitmapImage(new Uri(imgPath));
                bg.Source = img.Clone();
            }
            catch
            {

            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var exstyle = Win32.GetWindowLong(handle, Win32.GWL_EXSTYLE);
            exstyle |= Win32.WS_EX_NOACTIVATE;
            Win32.SetWindowLong(handle, Win32.GWL_EXSTYLE, exstyle);
            Task.Run(() =>
            {
                //headline.Text = "Title";
            });
        }

        internal void SwitchScreen(UserControl page)
        {
            if (page != null)
            {
                StackPanelMain.Children.Clear();
                StackPanelMain.Children.Add(page);
            }
        }

        private void MenuButton1_Click(object sender, RoutedEventArgs e)
        {
            SwitchScreen(pagePlay);
        }

        private void MenuButton2_Click(object sender, RoutedEventArgs e)
        {
            SwitchScreen(pageSheet);
        }
    }
}
