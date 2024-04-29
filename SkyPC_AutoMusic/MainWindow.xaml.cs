using MaterialDesignThemes.Wpf;
using Prism.Events;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.View;
using SkyPC_AutoMusic.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SkyPC_AutoMusic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //应为焦点的窗口
        public IntPtr lastActiveWindowHandle = IntPtr.Zero;

        double originalHeight;
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            //事件聚合器订阅
            EA.EventAggregator.GetEvent<WindowMinimizeEvent>().Subscribe(() => { WindowState = WindowState.Minimized; });
            EA.EventAggregator.GetEvent<WindowCloseEvent>().Subscribe(Close);
            EA.EventAggregator.GetEvent<WindowExpandEvent>().Subscribe(WindowExpand);
            EA.EventAggregator.GetEvent<AddBackgroundEvent>().Subscribe(AddBackground);
            EA.EventAggregator.GetEvent<DeleteBackgroundEvent>().Subscribe(DeleteBackground);
            EA.EventAggregator.GetEvent<SendMessageSnackbar>().Subscribe(SendMessageSnackbar);
            //初始化
            InitializeComponent();
            originalHeight = this.Height;
            Instance = this;
            //防止窗口成为焦点
            lastActiveWindowHandle = Win32.GetForegroundWindow();
            Deactivated += MainWindow_Deactivated;
            SourceInitialized += (object sender, EventArgs e) =>
            {
                var handle = new WindowInteropHelper(this).Handle;
                var exstyle = Win32.GetWindowLong(handle, Win32.GWL_EXSTYLE);
                exstyle |= Win32.WS_EX_NOACTIVATE;
                Win32.SetWindowLong(handle, Win32.GWL_EXSTYLE, exstyle);
            };
        }

        

        //修改背景图像
        private void AddBackground(BitmapImage img)
        {
            //string imgPath = AppDomain.CurrentDomain.BaseDirectory + "bg.jpg";
            //BitmapImage img = new BitmapImage(new Uri(imgPath));
            Body.Background = Brushes.Transparent;
            Background.Source = img;
        }

        //删除背景图像
        private void DeleteBackground()
        {
            Body.SetResourceReference(Control.BackgroundProperty, "MaterialDesignPaper");
            Background.Source = null;
        }

        //窗体位置拖动
        private void title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        //窗口展开或收起
        private void WindowExpand(bool isExpand)
        {
            if(isExpand)
            {
                Height = originalHeight;

                //DoubleAnimation animation = new DoubleAnimation();
                //animation.From = Height;
                //animation.To = originalHeight;
                //animation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                //BeginAnimation(Window.HeightProperty, animation);

                TitleBar.Children.Clear();
                TitleBar.Children.Add(new UserControlTitleBarNormal());
            }
            else
            {
                Height = TitleBar.ActualHeight;

                //DoubleAnimation animation = new DoubleAnimation();
                //animation.From = Height;
                //animation.To = TitleBar.ActualHeight;
                //animation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                //BeginAnimation(Window.HeightProperty, animation);

                TitleBar.Children.Clear();
                TitleBar.Children.Add(new UserControlTitleBarDetail());
            }
        }

        //消息框
        private void SendMessageSnackbar(string message)
        {
            var messageQueue = Snackbar.MessageQueue;
            messageQueue.Enqueue(message, Properties.Resources.Options_Confirm, (param) => { CloseMessage(); }, null, false, true, TimeSpan.FromMilliseconds(3000));
        }

        private void CloseMessage()
        {
            Snackbar.IsActive = false;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            lastActiveWindowHandle = Win32.GetForegroundWindow();
        }
    }
}
