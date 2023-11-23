using Newtonsoft.Json.Linq;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SkyPC_AutoMusic.Model
{
    public class Player
    {
        //进度条值
        public double SliderProgress
        {
            get
            {
                if (currentSong == null)//没有乐谱
                    return 0;

                double percentage = (DateTime.Now.Subtract(startPlay).TotalMilliseconds * speedModifier) / totalTimeProgress;
                return percentage;
            }
            set
            {
                if (currentSong == null)//没有乐谱
                    return;

                //double past = totalTimeProgress * value;

                //int minDiff = int.MaxValue;
                //int noteIndex = 0;
                //for (int i = 0; i < currentSong.songNotes.Count - 1; i++)
                //{
                //    double diff = Math.Abs(currentSong.songNotes[i].time - past);
                //    if (diff < minDiff)
                //    {
                //        minDiff = (int)diff;
                //        noteIndex = i;
                //    }
                //}
                //currentNoteIndex = noteIndex;

                //startPlay = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(past / speedModifier));

                currentBeatIndex = (int)((currentSong.Beats.Count - 1) * value);
                startPlay = DateTime.Now.AddMilliseconds(-currentSong.Beats[currentBeatIndex].Time * speedModifier);

                UpdateCurrentTimeLabel();
            }
        }

        //播放时间标签
        public string CurrentTime
        {
            get
            {
                //无乐谱或已播放完毕
                if (currentSong == null || isPlayEnd)
                    return currentTime;//直接返回缓存值
                //若处于暂停状态则不更新返回值
                if (!isStop)
                {
                    UpdateCurrentTimeLabel();
                }
                return currentTime;
            }
        }

        //乐谱总时长
        public string TotalTime
        {
            get
            {
                if (currentSong == null)//没有乐谱
                    return "00:00";

                TimeSpan ts = TimeSpan.FromMilliseconds(totalTimeProgress);
                return string.Format("{0:mm\\:ss}", ts);
            }
        }
        //倍数调节滑动条
        public double SliderSpeedModifier
        {
            get
            {
                return (speedModifier * 10f);
            }
            set
            {
                //写入倍速
                speedModifier = value * 0.1f;
                //判断播放状态
                if (currentSong != null && !isStop && !isPlayEnd)
                {
                    //播放时将倍速立刻应用
                    startPlay = DateTime.Now.AddMilliseconds(-currentSong.Beats[currentBeatIndex].Time / speedModifier);
                }
            }
        }

        //当前乐谱音乐
        public Song currentSong;
        //当前乐谱索引
        public int currentSheetIndex = 0;


        //当前时长
        private string currentTime;

        // 当前节拍索引
        private int currentBeatIndex;
        // 开始播放时间
        private DateTime startPlay;
        // 按下了停止
        public bool isStop = true;
        // 播放完当前乐谱
        public bool isPlayEnd;

        // 乐谱总时长
        private int totalTimeProgress = 0;
        // 按键持续时间
        private int durationTime = 50;
        // 倍数
        private double speedModifier = 1f;

        //键位
        private bool isUseSkyStudioKeyMapper;
        //延音
        private bool isDelayToReleaseKey;

        // 播放完的行为
        Action playEndAction;

        #region 公开方法

        public Player(Action playEndAction)
        {
            currentTime = "00:00";
            this.playEndAction = playEndAction;
            EA.EventAggregator.GetEvent<SwitchSkyStudioKeyMapperEvent>().Subscribe((flag) => 
            { 
                isUseSkyStudioKeyMapper = flag; 
            });
            EA.EventAggregator.GetEvent<SwitchDelayToReleaseEvent>().Subscribe((flag) =>
            {
                isDelayToReleaseKey = flag;
            });
        }

        public void InitializePlay()
        {
            currentTime = "00:00";
            currentBeatIndex = 0;
            startPlay = DateTime.Now;
            totalTimeProgress = currentSong.Beats[currentSong.Beats.Count - 1].Time;
        }

        /// <summary>
        /// 切换播放和暂停状态
        /// </summary>
        /// <returns>是否执行成功</returns>
        public bool TogglePlay()
        {
            if (currentSong == null)//没有谱子
                return false;
            
            if (isStop && !isPlayEnd)//暂停状态
            {
                //继续播放
                isStop = false;
                startPlay = DateTime.Now.AddMilliseconds(-currentSong.Beats[currentBeatIndex].Time / speedModifier);
                StartCycle();
            }
            else if (isPlayEnd)//播放完毕
            {
                //重新播放
                InitializePlay();
                isStop = false;
                StartCycle();
            }
            else if (!isPlayEnd)//播放状态
            {
                //暂停播放
                isStop = true;
            }

            return true;
        }

        #endregion

        #region 私有方法
        private void UpdateCurrentTimeLabel()
        {
            TimeSpan ts = DateTime.Now.Subtract(startPlay);
            ts = TimeSpan.FromMilliseconds(ts.TotalMilliseconds * speedModifier);
            currentTime = string.Format("{0:mm\\:ss}", ts);
        }

        private void StartCycle()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    //播放结束状态
                    isPlayEnd = currentBeatIndex == currentSong.Beats.Count;

                    if (!isPlayEnd && !isStop)
                    {
                        //正常播放
                        AutoPlay();
                    }
                    else if (isPlayEnd)
                    {
                        //播放完毕
                        isStop = true;
                        playEndAction();
                        break;
                    }
                    else
                    {
                        //手动暂停
                        break;
                    }

                    //抬起按键
                    if (isStop && currentBeatIndex > 0)
                    {
                        foreach (NoteKey key in currentSong.Beats[currentBeatIndex - 1].Keys)
                        {
                            SendKey(key, false);
                        }
                    }
                }
            });
        }

        private void AutoPlay()
        {
            //读取按键
            List<NoteKey> keys;
            if (currentSong != null)
                keys = currentSong.Beats[currentBeatIndex].Keys;
            else
                return;

            //判断时间
            int time = currentSong.Beats[currentBeatIndex].Time;
            bool flag = DateTime.Now > startPlay.AddMilliseconds(time / speedModifier);

            if (flag)
            {
                //按下
                foreach (NoteKey key in keys)
                {
                    SendKey(key, true);
                }

                //等待
                Thread.Sleep(durationTime);

                //抬起
                foreach (NoteKey key in keys)
                {
                    if (currentBeatIndex == currentSong.Beats.Count - 1 || !isDelayToReleaseKey)//最后一拍或关闭延音功能
                    {
                        //正常抬起
                        SendKey(key, false);
                        continue;
                    }
                    else if (!currentSong.Beats[currentBeatIndex + 1].Keys.Contains(key))//下一拍不包含音节
                    {
                        //延音抬起
                        SendKey(key, false);
                    }//下一拍包含音节则不抬起
                }

                //更新索引
                currentBeatIndex++;
            }
        }

        private void SendKey(NoteKey key, bool isPress)
        {
            byte bVk = 0;

            if (!isUseSkyStudioKeyMapper)
            {
                switch (key)//YUIOP
                {
                    case NoteKey.Key100:
                    case NoteKey.Key200:
                        bVk = 0x59;
                        break;
                    case NoteKey.Key101:
                    case NoteKey.Key201:
                        bVk = 0x55;
                        break;
                    case NoteKey.Key102:
                    case NoteKey.Key202:
                        bVk = 0x49;
                        break;
                    case NoteKey.Key103:
                    case NoteKey.Key203:
                        bVk = 0x4F;
                        break;
                    case NoteKey.Key104:
                    case NoteKey.Key204:
                        bVk = 0x50;
                        break;
                    case NoteKey.Key105:
                    case NoteKey.Key205:
                        bVk = 0x48;
                        break;
                    case NoteKey.Key106:
                    case NoteKey.Key206:
                        bVk = 0x4A;
                        break;
                    case NoteKey.Key107:
                    case NoteKey.Key207:
                        bVk = 0x4B;
                        break;
                    case NoteKey.Key108:
                    case NoteKey.Key208:
                        bVk = 0x4C;
                        break;
                    case NoteKey.Key109:
                    case NoteKey.Key209:
                        bVk = 0xBA;
                        break;
                    case NoteKey.Key110:
                    case NoteKey.Key210:
                        bVk = 0x4E;
                        break;
                    case NoteKey.Key111:
                    case NoteKey.Key211:
                        bVk = 0x4D;
                        break;
                    case NoteKey.Key112:
                    case NoteKey.Key212:
                        bVk = 0xBC;
                        break;
                    case NoteKey.Key113:
                    case NoteKey.Key213:
                        bVk = 0xBE;
                        break;
                    case NoteKey.Key114:
                    case NoteKey.Key214:
                        bVk = 0xBF;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (key)//QWERT
                {
                    case NoteKey.Key100:
                    case NoteKey.Key200:
                        bVk = 0x51;
                        break;
                    case NoteKey.Key101:
                    case NoteKey.Key201:
                        bVk = 0x57;
                        break;
                    case NoteKey.Key102:
                    case NoteKey.Key202:
                        bVk = 0x45;
                        break;
                    case NoteKey.Key103:
                    case NoteKey.Key203:
                        bVk = 0x52;
                        break;
                    case NoteKey.Key104:
                    case NoteKey.Key204:
                        bVk = 0x54;
                        break;
                    case NoteKey.Key105:
                    case NoteKey.Key205:
                        bVk = 0x41;
                        break;
                    case NoteKey.Key106:
                    case NoteKey.Key206:
                        bVk = 0x53;
                        break;
                    case NoteKey.Key107:
                    case NoteKey.Key207:
                        bVk = 0x44;
                        break;
                    case NoteKey.Key108:
                    case NoteKey.Key208:
                        bVk = 0x46;
                        break;
                    case NoteKey.Key109:
                    case NoteKey.Key209:
                        bVk = 0x47;
                        break;
                    case NoteKey.Key110:
                    case NoteKey.Key210:
                        bVk = 0x5A;
                        break;
                    case NoteKey.Key111:
                    case NoteKey.Key211:
                        bVk = 0x58;
                        break;
                    case NoteKey.Key112:
                    case NoteKey.Key212:
                        bVk = 0x43;
                        break;
                    case NoteKey.Key113:
                    case NoteKey.Key213:
                        bVk = 0x56;
                        break;
                    case NoteKey.Key114:
                    case NoteKey.Key214:
                        bVk = 0x42;
                        break;
                    default:
                        break;
                }
            }

            if (bVk != 0)
            {
                byte bScan = Win32.MapVirtualKey(bVk, 0);

                if (isPress)
                {
                    //按下
                    Win32.keybd_event(bVk, bScan, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
                }
                else
                {
                    //释放
                    Win32.keybd_event(bVk, bScan, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }
        }

        #endregion
    }
}
