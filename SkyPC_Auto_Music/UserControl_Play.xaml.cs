using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using MaterialDesignThemes.Wpf;
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
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace SkyPC_Auto_Music
{
    /// <summary>
    /// PagePlay.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Play : UserControl
    {
        //主窗体
        MainWindow _context;

        // 当前乐谱
        Song currentSheet;
        // 当前音节索引
        int currentNoteIndex;

        // 开始播放时间
        DateTime startPlay;
        // 按下了停止
        bool isStop = false;

        // 乐谱播放时间
        int totalTimeProgress = 0;
        // 按键持续时间
        int durationTime = 50;
        // 倍数
        float speedModifier = 1f;

        object lockObj = new object();

        public UserControl_Play(MainWindow context)
        {
            InitializeComponent();
            _context = context;
        }

        public void StartPlay(object sender, RoutedEventArgs e)
        {
            try
            {
                // 读取乐谱
                string path = _context.pageSheet.filePath.Text;
                currentSheet = ReadJson(path);
            }
            catch (Exception error)
            {
                MessageTips("请检查乐谱文件路径是否正确(不支持读取加密乐谱)\n" + error.Message, sender, e);
                return;
            }

            // 初始化播放
            isStop = false;
            startPlay = DateTime.Now;
            currentNoteIndex = 0;
            totalTimeProgress = currentSheet.songNotes[currentSheet.songNotes.Count - 1].time;
            //开始循环
            Task.Run(() =>
            {
                lock (lockObj)
                {
                    while (true)
                    {
                        if (currentNoteIndex < currentSheet.songNotes.Count && !isStop)
                            AutoPlay();
                        else
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// 读取乐谱Json文件
        /// </summary>
        /// <param name="path">文件路径</param>
        private Song ReadJson(string path)
        {
            // 读取文件内容  
            string rawJson = File.ReadAllText(path);
            // 掐头掐尾
            string json = rawJson.Substring(1, rawJson.Length - 3);
            // 反序列化成C#对象  
            return JsonConvert.DeserializeObject<Song>(json);
        }

        private void AutoPlay()
        {
            bool flag;
            if (speedModifier == 1f)//正常倍速
                flag = DateTime.Now > startPlay.AddMilliseconds(currentSheet.songNotes[currentNoteIndex].time);
            else//修改了倍速
                flag = DateTime.Now > startPlay.AddMilliseconds(currentSheet.songNotes[currentNoteIndex].time / speedModifier);

            if (flag)
            {
                int sum = 0;
                //缓存索引(防止Sleep时修改进度后按键不抬起)
                int noteIndex = currentNoteIndex;
                //按下
                do
                {
                    //按下
                    SendKey(currentSheet.songNotes[noteIndex + sum].key, true);
                    //更新索引
                    sum++;
                    if (noteIndex + sum >= currentSheet.songNotes.Count - 1)
                    {
                        break;
                    }
                }
                while (currentSheet.songNotes[noteIndex + sum - 1].time == currentSheet.songNotes[noteIndex + sum].time);
                //等待
                Thread.Sleep(durationTime);
                //松开
                for (int i = 0; i < sum; i++)
                {
                    //松开
                    SendKey(currentSheet.songNotes[noteIndex + i].key, false);
                }
                //显示进度
                if (currentSheet.songNotes[noteIndex].time != 0)
                {
                    float percentage = ((float)currentSheet.songNotes[noteIndex].time / (float)totalTimeProgress) * 100f;
                    _context.Dispatcher.Invoke(new Action(() => {
                        _context.Title = percentage.ToString("F2") + " % (" + currentSheet.songNotes[noteIndex].time.ToString() + " - " + totalTimeProgress + ")";
                        _context.pagePlay.sheetProgress.Value = (int)percentage;
                    }));
                    //_context.Title = percentage.ToString("F2") + " % (" + currentSheet.songNotes[noteIndex].time.ToString() + " - " + totalTimeProgress + ")";
                    //_context.pagePlay.sheetProgress.Value = (int)percentage;
                }
                //更新索引
                currentNoteIndex += sum;
            }

        }

        /// <summary>
        /// 发送按键信号
        /// </summary>
        /// <param name="keySymbol">虚拟键码按键压或释放</param>
        private void SendKey(string keySymbol, bool isPress)
        {
            byte pKey = 0;

            switch (keySymbol)
            {
                case "1Key0":
                case "2Key0":
                    pKey = 0x59;
                    break;
                case "1Key1":
                case "2Key1":
                    pKey = 0x55;
                    break;
                case "1Key2":
                case "2Key2":
                    pKey = 0x49;
                    break;
                case "1Key3":
                case "2Key3":
                    pKey = 0x4F;
                    break;
                case "1Key4":
                case "2Key4":
                    pKey = 0x50;
                    break;
                case "1Key5":
                case "2Key5":
                    pKey = 0x48;
                    break;
                case "1Key6":
                case "2Key6":
                    pKey = 0x4A;
                    break;
                case "1Key7":
                case "2Key7":
                    pKey = 0x4B;
                    break;
                case "1Key8":
                case "2Key8":
                    pKey = 0x4C;
                    break;
                case "1Key9":
                case "2Key9":
                    pKey = 0xBA;
                    break;
                case "1Key10":
                case "2Key10":
                    pKey = 0x4E;
                    break;
                case "1Key11":
                case "2Key11":
                    pKey = 0x4D;
                    break;
                case "1Key12":
                case "2Key12":
                    pKey = 0xBC;
                    break;
                case "1Key13":
                case "2Key13":
                    pKey = 0xBE;
                    break;
                case "1Key14":
                case "2Key14":
                    pKey = 0xBF;
                    break;
                default:
                    break;
            }

            if (pKey != 0)
            {
                if (isPress)
                {
                    //按下
                    Win32.keybd_event(pKey, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                }
                else
                {
                    //释放
                    Win32.keybd_event(pKey, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }
        }

        public async void MessageTips(string message, object sender, RoutedEventArgs e)
        {
            var sampleMessageDialog = new SimpleMessageDialog
            {
                Message = { Text = message }
            };
            await DialogHost.Show(sampleMessageDialog, "RootDialog");
        }

        private void sheetProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (currentSheet == null)
            //    return;
            //currentNoteIndex = (int)((float)(currentSheet.songNotes.Count - 1) * (float)sheetProgress.Value * 0.01f);
            //startPlay = DateTime.Now.AddMilliseconds(-currentSheet.songNotes[currentNoteIndex].time);
        }

        private void Pause(object sender, RoutedEventArgs e)
        {
            if (currentSheet == null)
                return;

            if (isStop && currentNoteIndex < currentSheet.songNotes.Count)
            {
                isStop = false;
                PauseButton.Content = "暂停";
                startPlay = DateTime.Now.AddMilliseconds(-currentSheet.songNotes[currentNoteIndex].time / speedModifier);
                // 开始循环
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (currentNoteIndex < currentSheet.songNotes.Count && !isStop)
                            AutoPlay();
                        else
                            break;
                    }
                });
            }
            else if (currentNoteIndex < currentSheet.songNotes.Count)
            {
                isStop = true;
                PauseButton.Content = "恢复";
            }
        }
    }
}
