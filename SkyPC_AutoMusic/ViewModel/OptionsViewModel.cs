using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using SkyPC_AutoMusic.Command;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.Event.Options;
using SkyPC_AutoMusic.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class OptionsViewModel : NotificationObject
    {
        private string url = "https://github.com/qwe5283/SkyPC_AutoMusic";

        private Settings settings;

        private ComboBox LanguageComboBox;

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
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(Properties.Resources.Options_Tips_RestartApp);
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
                ChangeBackground(value,true);
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
                    SendDialog.MessageTips(Properties.Resources.Options_Tips_CombineKeys);
                OnPropertyChanged();
                Save();
            }
        }

        //后台演奏
        public bool IsPlayInBackground
        {
            get { return settings.isPlayInBackground; }
            set
            {
                settings.isPlayInBackground = value;
                EA.EventAggregator.GetEvent<SwitchPlayBackgroundEvent>().Publish(value);
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
        public DelegateCommand LanguageChangedCommand { get; set; }

        public OptionsViewModel(ComboBox languageComboBox)
        {
            //读取设置
            settings = Settings.Instance;
            Read();
            //语言框
            LanguageComboBox = languageComboBox;
            var languages = new List<LanguageItem>
            {
                new LanguageItem { DisplayName = "Auto", LanguageCode = null },
                new LanguageItem { DisplayName = "中文", LanguageCode = "zh-CN" },
                new LanguageItem { DisplayName = "English", LanguageCode = "en-US" }
            };
            LanguageComboBox.ItemsSource = languages;
            LanguageComboBox.SelectedIndex = languages.FindIndex(l => l.LanguageCode == settings.LanguageCode);
            //命令绑定
            OpenWebPageCommand = new DelegateCommand(OpenWebPage);
            LanguageChangedCommand = new DelegateCommand(LanguageComboBoxChanged);
            EA.EventAggregator.GetEvent<SaveFolderPathEvent>().Subscribe(SaveFolderPath);
            //应用读取的设置
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
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(Properties.Resources.Options_ReadConfigFailure);
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
                    EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(Properties.Resources.Options_ReadPathFailure);
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
            //初始化键位
            EA.EventAggregator.GetEvent<SwitchSkyStudioKeyMapperEvent>().Publish(IsUsingSkyStudioKeyMapper);
            //初始化延音
            EA.EventAggregator.GetEvent<SwitchDelayToReleaseEvent>().Publish(DelayToReleaseKey);
            //初始化后台演奏
            EA.EventAggregator.GetEvent<SwitchPlayBackgroundEvent>().Publish(IsPlayInBackground);
            //背景图像
            ChangeBackground(UserImageBackground,false);
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
        private void ChangeBackground(bool useBackground,bool sendFailureMessage)
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
                    }
                }
                else//图片不存在
                {
                    if (sendFailureMessage)
                    {
                        SendDialog.MessageTips(Properties.Resources.Options_Tips_MissBackgroundFile);
                    }
                    else
                    {
                        //EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(Properties.Resources.Options_Tips_MissBackgroundFile);
                    }
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
        private void OpenWebPage()
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }

        //切换语言
        private void LanguageComboBoxChanged()
        {
            ResourceManager resourceManager = new ResourceManager("SkyPC_AutoMusic.Properties.Resources",typeof(OptionsViewModel).Assembly);
            LanguageItem selectedItem = (LanguageItem)LanguageComboBox.SelectedItem;
            string tips;
            if (selectedItem.LanguageCode != null)
            {
                tips = resourceManager.GetString("Options_Tips_RestartApp", new CultureInfo(selectedItem.LanguageCode));
            }
            else
            {
                tips = Properties.Resources.Options_Tips_RestartApp;
            }
            EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(tips);
            settings.LanguageCode = selectedItem.LanguageCode;
            Save();
        }

    }
}
