using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using SkyPC_AutoMusic.Command;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class OptionsViewModel : NotificationObject
    {
        private string url = "https://space.bilibili.com/501415702";

        private Settings settings;

        //乐谱文件夹路径
        public string FolderPath
        {
            get { return settings.FolderPath; }
            set 
            {
                settings.FolderPath = value;
                OnPropertyChanged();
                Save();
            }
        }

        //深色模式
        public bool DarkTheme
		{
			get { return settings.DarkTheme; }
			set 
            { 
                settings.DarkTheme = value;
                ToggleDarkMode(value);
                OnPropertyChanged();
                Save();
            }
		}

        //主题色跟随系统
        public bool ThemeColorFollowSystem
        {
            get { return settings.ThemeColorFollowSystem; }
            set
            {
                settings.ThemeColorFollowSystem = value;
                if (value)
                {
                    //系统接管主题色
                    Microsoft.Win32.SystemEvents.UserPreferenceChanged += SwitchThemeColor;
                    SwitchThemeColor(null, null);
                }
                else
                {
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish("默认主题色将在程序重启后生效");
                }
                OnPropertyChanged();
                Save();
            }
        }

        //自定义背景图像
        public bool UserImageBackground
        {
            get { return settings.UserImageBackground; }
            set
            {
                settings.UserImageBackground = value;
                ChangeBackground(value);
                OnPropertyChanged();
                Save();
            }
        }

        //延迟释放按键
        public bool DelayToReleaseKey
        {
            get { return settings.DelayToReleaseKey; }
            set
            {
                settings.DelayToReleaseKey = value;
                EA.EventAggregator.GetEvent<SwitchDelayToReleaseEvent>().Publish(value);
                if (value)
                    SendDialog.MessageTips("部分乐谱与此功能不兼容，若演奏过程出现意外的缺音，请关闭此功能");
                OnPropertyChanged();
                Save();
            }
        }

        //键位映射
        public bool IsUsingSkyStudioKeyMapper
        {
            get { return settings.isUsingSkyStudioKeyMapper; }
            set
            {
                settings.isUsingSkyStudioKeyMapper = value;
                EA.EventAggregator.GetEvent<SwitchSkyStudioKeyMapperEvent>().Publish(value);
                OnPropertyChanged();
                Save();
            }
        }

        public DelegateCommand OpenWebPageCommand { get; set; }

        public OptionsViewModel()
        {
            settings = Settings.Instance;
            OpenWebPageCommand = new DelegateCommand(OpenWebPage);
            EA.EventAggregator.GetEvent<SaveFolderPathEvent>().Subscribe(SaveFolderPath);
            Read();
            ApplyAllOptions();
        }

        //读取设置
        private void Read()
        {
            //读取首选项
            string filePath = "settings.xml";
            if (File.Exists(filePath))//存在则直接读取
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        settings = (Settings)(serializer.Deserialize(reader));
                    }
                }
                catch
                {
                    Save();
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish("设置文件读取失败，所有设置项已重置为默认值");
                }
            }
        }

        //保存设置
        private void Save()
		{
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (StreamWriter writer = new StreamWriter("settings.xml"))
            {
                serializer.Serialize(writer, settings);
            }
        }

        //主题色
        private void SwitchThemeColor(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            var palette = new PaletteHelper();
            ITheme theme = palette.GetTheme();
            theme.SetPrimaryColor(((SolidColorBrush)SystemParameters.WindowGlassBrush).Color);
            palette.SetTheme(theme);
        }

        //将乐谱文件夹写入设置
        private void SaveFolderPath(string path)
        {
            FolderPath = path;
        }

        //应用所有设置
        private void ApplyAllOptions()
        {
            //从文件夹导入乐谱
            if (FolderPath != null)
            {
                if (Directory.Exists(FolderPath))
                {
                    EA.EventAggregator.GetEvent<SelectFolderWithPathEvent>().Publish(FolderPath);
                }
                else
                {
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish("从设置文件读取路径失败");
                }
            }
            //深色模式
            ToggleDarkMode(DarkTheme);
            //系统接管主题色
            if(ThemeColorFollowSystem)
            {
                Microsoft.Win32.SystemEvents.UserPreferenceChanged += SwitchThemeColor;
                SwitchThemeColor(null, null);
            }
            //键位
            EA.EventAggregator.GetEvent<SwitchSkyStudioKeyMapperEvent>().Publish(IsUsingSkyStudioKeyMapper);
            //延音
            EA.EventAggregator.GetEvent<SwitchDelayToReleaseEvent>().Publish(DelayToReleaseKey);
            //背景图像
            ChangeBackground(UserImageBackground);
        }

        //切换深色模式
        private void ToggleDarkMode(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
        }

        //切换背景图像
        private void ChangeBackground(bool useBackground)
        {
            bool fileExist;
            string imgPath = AppDomain.CurrentDomain.BaseDirectory + "bg.jpg";
            fileExist = File.Exists(imgPath);
            if (!fileExist)
                imgPath = AppDomain.CurrentDomain.BaseDirectory + "bg.png";
            fileExist = File.Exists(imgPath);
            if (useBackground)//添加背景图片
            {
                if (fileExist)//图片存在
                {
                    //读取图片
                    BitmapImage bitmap;
                    using (FileStream fs = new FileStream(imgPath, FileMode.Open))
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = fs;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                    //切换背景图像
                    EA.EventAggregator.GetEvent<AddBackgroundEvent>().Publish(bitmap);
                    //切换深色模式
                    if (ShouldUseDarkMode(bitmap) != DarkTheme)
                    {
                        DarkTheme = !DarkTheme;
                        EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish("深色模式设置项已由程序修改");
                    }
                }
                else//图片不存在
                {
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish("未找到背景图片，请将bg.jpg(或bg.png)与程序放在同一目录下");
                }
            }
            else//删除背景图片
            {
                EA.EventAggregator.GetEvent<DeleteBackgroundEvent>().Publish();
            }
            
        }

        //根据背景决定深色模式
        public bool ShouldUseDarkMode(BitmapImage bitmap)
        {
            {
                try
                {
                    // 定义一个变量，存储图片的总亮度
                    double brightness = 0;

                    // 获取图片的像素宽度和高度
                    int width = bitmap.PixelWidth;
                    int height = bitmap.PixelHeight;

                    // 获取图片的像素数据
                    byte[] pixels = new byte[width * height * 4];
                    bitmap.CopyPixels(pixels, width * 4, 0);

                    // 遍历每个像素
                    for (int i = 0; i < pixels.Length; i += 4)
                    {
                        // 获取像素的蓝色、绿色和红色分量
                        byte blue = pixels[i];
                        byte green = pixels[i + 1];
                        byte red = pixels[i + 2];

                        // 计算像素的亮度，使用公式：Y = 0.299 * R + 0.587 * G + 0.114 * B
                        double y = 0.299 * red + 0.587 * green + 0.114 * blue;

                        // 累加像素的亮度
                        brightness += y;
                    }

                    // 计算图片的平均亮度
                    brightness /= (width * height);

                    // 如果平均亮度小于128，则返回true，表示文本前景色应为白色
                    // 否则，返回false，表示文本前景色应为黑色
                    return brightness < 128;
                }
                catch
                {
                    return true;
                }
            }
        }

        //打开网页
        public void OpenWebPage()
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }


    }
}
