using SkyPC_AutoMusic.Command;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Threading;
using SkyPC_AutoMusic.View;
using Prism.Events;

namespace SkyPC_AutoMusic.Model
{
    internal class PlayViewModel : NotificationObject
    {
        //播放模式
        private enum PlayEndMode
        {
            StopPlay , SinglePlay ,ListPlay
        }

        private PlayEndMode playEndMode;

        //播放器
        private Player player;

        //乐谱数
        private int sheetsCount;

        //是否正在播放最后一首歌
        private bool isLastSong { get {  return player.currentSheetIndex == sheetsCount - 1; } }

        //播放页标题
        private string headline;

        //UI更新
        private DispatcherTimer timer;

        #region 公开属性

        //进度条值
        public double SliderProgress
        {
            get { return player.SliderProgress; }
            set 
            {
                player.SliderProgress = value; 
                OnPropertyChanged("CurrentTime"); 
            }
        }

        //标题文字
        public string Headline
        {
            get { return headline; }
            set { headline = value; OnPropertyChanged(); }
        }

        //当前播放时长
        public string CurrentTime
        {
            get { return player.CurrentTime; }
        }

        //乐谱总时长
        public string TotalTime
        {
            get { return player.TotalTime; }
        }

        //暂停按钮图标
        public PackIconKind TogglePlayButtonIcon
        {
            get
            {
                if (!player.isStop)
                    return PackIconKind.Pause;
                else
                    return PackIconKind.Play;
            }
        }

        //暂停按钮标签
        public string TogglePlayButtonLabel
        {
            get
            {
                if (!player.isStop || player.currentSong == null)
                    return "暂停";
                else
                    return "播放";
            }
        }

        //倍数调节滑动条
        public double SliderSpeedModifier
        {
            get { return (int)player.SliderSpeedModifier; }
            set { player.SliderSpeedModifier = value; }
        }

        //播放模式标签
        public string PlayModeLabel
        {
            get
            {
                string str = string.Empty;
                switch (playEndMode)
                {
                    case PlayEndMode.StopPlay:
                        str = "播完暂停";
                        break;
                    case PlayEndMode.SinglePlay:
                        str = "单曲循环";
                        break;
                    case PlayEndMode.ListPlay:
                        str = "顺序播放";
                        break;
                    default:
                        break;
                }
                return str;
            }
        }

        public DelegateCommand PreviousSongCommand { get; set; }

        public DelegateCommand SwitchModeCommand { get; set; }

        public DelegateCommand TogglePlayCommand { get; set; }

        public DelegateCommand NextSongCommand { get; set; }

        public DelegateCommand ShowSheetInfoCommand { get; set; }

        #endregion

        public PlayViewModel()
        {
            //事件聚合器
            EA.EventAggregator.GetEvent<HeadlineSwitchEvent>().Subscribe(ChangeHeadline);
            EA.EventAggregator.GetEvent<PassSheetsCountEvent>().Subscribe(UpdateSheetsCount);
            EA.EventAggregator.GetEvent<FolderSwitchEvent>().Subscribe(OnFolderSwitch);
            EA.EventAggregator.GetEvent<SongSwitchWithIndexEvent>().Subscribe(SongSwitchWithIndex);
            EA.EventAggregator.GetEvent<NextPreviousSongEvent>().Subscribe(ToggleSong);
            EA.EventAggregator.GetEvent<PauseSongEvent>().Subscribe(TogglePlay);
            EA.EventAggregator.GetEvent<RequirePlayViewModelEvent>().Subscribe(PassPlayViewModel);
            //私有变量
            player = new Player(PlayEndAction);
            playEndMode = new PlayEndMode();
            playEndMode = PlayEndMode.StopPlay;
            //公开属性
            Headline = "SkyPC AutoMusic";
            //命令
            PreviousSongCommand = new DelegateCommand(() => { ToggleSong(false); });
            SwitchModeCommand = new DelegateCommand(SwitchMode);
            TogglePlayCommand = new DelegateCommand(TogglePlay);
            NextSongCommand = new DelegateCommand(() => { ToggleSong(true); });
            ShowSheetInfoCommand = new DelegateCommand(ShowSheetInfo);
            //更新UI
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += (object sender, EventArgs e) => 
            {
                if (!player.isStop)
                    UpdatePlayerUI(); 
            };
            timer.Start();
        }

        private void PassPlayViewModel()
        {
            EA.EventAggregator.GetEvent<GetPlayViewModelEvent>().Publish(this);
        }

        private void SwitchMode()
        {
            if (playEndMode != PlayEndMode.ListPlay)
                playEndMode += 1;
            else
                playEndMode = 0;

            OnPropertyChanged("PlayModeLabel");
            OnPropertyChanged("PlayModeIcon");
        }

        private void ToggleSong(bool isNextSong)
        {
            if (isNextSong)//下一首
            {
                if (sheetsCount == 0 || isLastSong)//播放列表没有音乐或已经是最后一首
                {
                    SendDialog.MessageTips("已经是最后一首音乐");
                }
                else
                {
                    player.currentSheetIndex++;
                    //切歌
                    EA.EventAggregator.GetEvent<SongSwitchEvent>().Publish(player);
                    //初始化播放
                    player.InitializePlay();
                    //更新播放器UI
                    UpdatePlayerUI();
                }
            }
            else//上一首
            {
                //已经是第一首
                if (player.currentSheetIndex == 0)
                {
                    SendDialog.MessageTips("已经是第一首音乐");
                }
                else
                {
                    player.currentSheetIndex--;
                    //切歌
                    EA.EventAggregator.GetEvent<SongSwitchEvent>().Publish(player);
                    //初始化播放
                    player.InitializePlay();
                    //更新播放器UI
                    UpdatePlayerUI();
                }
            }
        }

        private void TogglePlay()
        {
            if (!player.TogglePlay())
            {
                SendDialog.MessageTips("还没有选择乐谱");
            }
            //暂停按钮标签
            OnPropertyChanged("TogglePlayButtonIcon");
            OnPropertyChanged("TogglePlayButtonLabel");
            //标题栏暂停按钮
        }

        private void OnFolderSwitch()
        {
            player.currentSheetIndex = 0;
            EA.EventAggregator.GetEvent<SongSwitchEvent>().Publish(player);
            player.InitializePlay();
        }

        private void ChangeHeadline(string songName)
        {
            Headline = songName;
        }

        private void UpdateSheetsCount(int SheetsCount)
        {
            sheetsCount = SheetsCount;
        }

        private void SongSwitchWithIndex(int index)
        {
            player.currentSheetIndex = index;
            EA.EventAggregator.GetEvent<SongSwitchEvent>().Publish(player);
            player.InitializePlay();
            UpdatePlayerUI();
        }

        private void UpdatePlayerUI()
        {
            //播放页UI
            OnPropertyChanged("SliderProgress");
            OnPropertyChanged("CurrentTime");
            OnPropertyChanged("TotalTime");
            //标题栏UI
            EA.EventAggregator.GetEvent<SynchronizeDetailedTitleBarEvent>().Publish();
        }

        private void ShowSheetInfo()
        {
            if (player.currentSong != null)
            {
                string info = "歌曲详情\n";
                info += "\n曲名: " + player.currentSong.name;
                info += "\n作者: " + player.currentSong.author;
                info += "\n改编者: " + player.currentSong.transcribedBy;

                string pitchLevel = string.Empty;
                switch (player.currentSong.pitchLevel)
                {
                    case 0:
                        pitchLevel = "C";
                        break;
                    case 1:
                        pitchLevel = "D♭";
                        break;
                    case 2:
                        pitchLevel = "D";
                        break;
                    case 3:
                        pitchLevel = "E♭";
                        break;
                    case 4:
                        pitchLevel = "E";
                        break;
                    case 5:
                        pitchLevel = "F";
                        break;
                    case 6:
                        pitchLevel = "G♭";
                        break;
                    case 7:
                        pitchLevel = "G";
                        break;
                    case 8:
                        pitchLevel = "A♭";
                        break;
                    case 9:
                        pitchLevel = "A";
                        break;
                    case 10:
                        pitchLevel = "B♭";
                        break;
                    case 11:
                        pitchLevel = "B";
                        break;
                    default:
                        break;
                }
                info += "\n调性: " + pitchLevel;

                SendDialog.MessageTips(info);
            }
        }

        private void PlayEndAction()
        {
            switch (playEndMode)
            {
                case PlayEndMode.StopPlay:
                    UpdatePlayerUI();
                    break;
                case PlayEndMode.SinglePlay:
                    TogglePlay();
                    UpdatePlayerUI();
                    break;
                case PlayEndMode.ListPlay:
                    if (!isLastSong)
                    {
                        ToggleSong(true);
                        TogglePlay();
                    }
                    UpdatePlayerUI();
                    break;
                default:
                    break;
            }
            OnPropertyChanged("TogglePlayButtonIcon");
            OnPropertyChanged("TogglePlayButtonLabel");
        }
    }
}
