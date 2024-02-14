using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_AutoMusic.Model
{

    public class Settings
    {
        //单例
        private static readonly Settings instance = new Settings();

        private Settings() { }

        static Settings() { }

        public static Settings Instance
        {
            get { return instance; }
        }

        //私有变量
        private string folderPath;
        private string languageCode;
        private bool darkTheme;
        private bool themeColorFollowSystem;
        private bool userImageBackground;
        private bool delayToReleaseKey;
        private bool skyStudioKeyMapper;

        //文件夹路径
        public string FolderPath
        {
            get { return folderPath; }
            set { folderPath = value; }
        }

        //语言
        public string LanguageCode
        {
            get { return languageCode; }
            set { languageCode = value; }
        }

        //深色模式
        public bool DarkTheme
        {
            get { return darkTheme; }
            set { darkTheme = value; }
        }

        //主题色跟随系统
        public bool ThemeColorFollowSystem
        {
            get { return themeColorFollowSystem; }
            set { themeColorFollowSystem = value; }
        }

        //自定义背景图像
        public bool UserImageBackground
        {
            get { return userImageBackground; }
            set { userImageBackground = value; }
        }

        //按键延音
        public bool DelayToReleaseKey
        {
            get { return delayToReleaseKey; }
            set { delayToReleaseKey = value; }
        }

        //键位映射
        public bool isUsingSkyStudioKeyMapper
        {
            get { return skyStudioKeyMapper; ; }
            set { skyStudioKeyMapper = value; }
        }
    }
}
