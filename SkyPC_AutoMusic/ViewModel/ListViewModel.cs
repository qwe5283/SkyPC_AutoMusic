using SkyPC_AutoMusic.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Prism.Events;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.Model;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using SkyPC_AutoMusic.View;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class ListViewModel : NotificationObject
    {

        #region 公开属性

        private ObservableCollection<Song> sheetList;

        public ObservableCollection<Song> SheetList
        {
            get { return sheetList; }
            set { sheetList = value; OnPropertyChanged(); }
        }

        private Song selectedItem;

        public Song SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; }
        }

        public DelegateCommand AddSongCommand { get; set; }

        public DelegateCommand DeleteSelectionCommand { get; set; }

        public DelegateCommand SelectFolderCommand { get; set; }

        public DelegateCommand SelectionChangeCommand { get; set; }

        #endregion

        public ListViewModel()
        {
            //私有变量
            EA.EventAggregator.GetEvent<SongSwitchEvent>().Subscribe(SwitchCurrentSong);
            EA.EventAggregator.GetEvent<SelectFolderWithPathEvent>().Subscribe(AutoSelectFolder);
            //公开属性
            sheetList = new ObservableCollection<Song>();
            //命令
            DeleteSelectionCommand = new DelegateCommand(DeleteSelection);
            SelectFolderCommand = new DelegateCommand(SelectFolder);
            SelectionChangeCommand = new DelegateCommand(SelectionChange);
            AddSongCommand = new DelegateCommand(AddSong);
        }

        #region 私有方法
        private void SelectionChange()
        {
            //获取选中乐谱的索引
            int songIndex = sheetList.IndexOf(SelectedItem);
            if (songIndex != -1)
            {
                EA.EventAggregator.GetEvent<SongSwitchWithIndexEvent>().Publish(songIndex);
            }
        }

        private void SwitchCurrentSong(Player player)
        {
            //切歌
            player.currentSong = sheetList[player.currentSheetIndex];
            //更新标题
            EA.EventAggregator.GetEvent<HeadlineSwitchEvent>().Publish(sheetList[player.currentSheetIndex].name);
        }

        private void SelectFolder()
        {
            //打开对话框
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ReadSheetsFromFolder(dialog.SelectedPath, true);
                EA.EventAggregator.GetEvent<SaveFolderPathEvent>().Publish(dialog.SelectedPath);
            }
        }

        private void ReadSheetsFromFolder(string folderPath,bool showDialog)
        {
            //清理列表
            sheetList.Clear();
            //反馈信息
            int successCount = 0;
            int failCount = 0;
            string infoFeedback = String.Empty;
            string failureList = String.Empty;
            string[] files = Directory.GetFiles(folderPath);
            int total = 0;
            int totalCount = files.Count(file => Path.GetExtension(file).ToLower() == ".txt");
            //禁用列表视图
            EA.EventAggregator.GetEvent<EnableListEvent>().Publish(false);
            //显示等待框
            if (showDialog)
            {
                var dialog = SendDialog.WaitTips("导入乐谱中，请耐心等候", totalCount);
            }
            else
            {
                EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(String.Format("正在导入{0}首乐谱",totalCount));
            }
            Task.Run(() =>
            {
                //遍历文件
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".txt")
                    {
                        try
                        {
                            Sheet song;
                            //从文件实例化类
                            try
                            {
                                song = ReadJson(file);
                            }
                            catch
                            {
                                song = ReReadJson(file);
                            }
                            //计数
                            successCount++;
                            //添加至列表
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                SheetList.Add(ConvertSheetToSong(song));
                            });
                        }
                        catch
                        {
                            failCount++;
                            failureList += ("\n\n" + file);
                        }

                        total++;
                        EA.EventAggregator.GetEvent<PassProgressToWaitDialogEvent>().Publish(total);
                    }
                }

            }).ContinueWith((preTask) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    //启用列表视图
                    EA.EventAggregator.GetEvent<EnableListEvent>().Publish(true);
                    //反馈信息
                    infoFeedback = string.Format("{0}个乐谱导入成功,{1}个乐谱导入失败", successCount, failCount);
                    if (showDialog)
                    {
                        if (failCount != 0)
                        {
                            infoFeedback += "\n\n(目前仅支持导入未加密Json乐谱)\n\n以下是导入失败的文件:";
                            infoFeedback += failureList;
                        }
                        SendDialog.MessageTips(infoFeedback);
                    }
                    else
                    {
                        EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(infoFeedback);
                    }
                    if (sheetList.Count > 0)
                    {
                        //传递乐谱数
                        EA.EventAggregator.GetEvent<PassSheetsCountEvent>().Publish(sheetList.Count);
                        //切歌
                        EA.EventAggregator.GetEvent<FolderSwitchEvent>().Publish();
                    }
                    
                });
            });
            
        }

        private Sheet ReadJson(string path)
        {
            // 读取文件内容  
            string rawJson = File.ReadAllText(path);
            // 掐头掐尾
            string json = rawJson.Substring(1, rawJson.Length - 3);
            // 反序列化成C#对象  
            return JsonConvert.DeserializeObject<Sheet>(json);
        }

        private Sheet ReReadJson(string path)
        {
            // 读取文件内容  
            string rawJson = File.ReadAllText(path);
            // 掐头掐尾
            string json = rawJson.Substring(1, rawJson.Length - 4);
            // 反序列化成C#对象  
            return JsonConvert.DeserializeObject<Sheet>(json);
        }

        private void AddSong()
        {
            //打开对话框
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "请选择乐谱";
            dialog.Filter = "文本文件(*.txt)|*.txt";
            //反馈信息
            string infoFeedback = string.Empty;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Sheet song;
                    //从文件实例化类
                    try
                    {
                        song = ReadJson(dialog.FileName);
                    }
                    catch
                    {
                        song = ReReadJson(dialog.FileName);
                    }
                    //添加至列表
                    SheetList.Add(ConvertSheetToSong(song));
                    infoFeedback = string.Format("{0} 导入成功", song.name);
                }
                catch
                {
                    infoFeedback = string.Format("乐谱导入失败,请检查乐谱格式是否正确\n\n(目前仅支持导入未加密Json乐谱)");
                }
                //传递乐谱数
                EA.EventAggregator.GetEvent<PassSheetsCountEvent>().Publish(sheetList.Count);
                //切歌
                EA.EventAggregator.GetEvent<FolderSwitchEvent>().Publish();
                SendDialog.MessageTips(infoFeedback);
            }
        }

        private void DeleteSelection()
        {
            if (SelectedItem != null)
            {
                int songIndex = sheetList.IndexOf(SelectedItem) - 1;
                SheetList.Remove(SelectedItem);
                if (songIndex > -1)
                {
                    SelectedItem = sheetList[songIndex];
                    EA.EventAggregator.GetEvent<SongSwitchWithIndexEvent>().Publish(songIndex);
                }
            }
            else
            {
                SendDialog.MessageTips("你还没选择乐谱");
            }
        }

        private void AutoSelectFolder(string folderPath)
        {
            ReadSheetsFromFolder(folderPath, false);
        }

        private Song ConvertSheetToSong(Sheet sheet)
        {
            //基本属性
            Song song = new Song
            {
                name = sheet.name,
                author = sheet.author,
                transcribedBy = sheet.transcribedBy,
                isComposed = sheet.isComposed,
                bpm = sheet.bpm,
                bitsPerPage = sheet.bitsPerPage,
                pitchLevel = sheet.pitchLevel,
                isEncrypted = sheet.isEncrypted,
                Beats = new List<Beat>()
            };

            //每一拍的时间间隔
            int singleBeatInterval = 60000 / sheet.bpm;

            //拍数
            int LastBeatsCount = sheet.songNotes.Max(item => item.time) / singleBeatInterval;//sheet.songNotes[sheet.songNotes.Count - 1].time / singleBeatInterval;
            int totalBeatsCount = ((LastBeatsCount + song.bitsPerPage - 1) / song.bitsPerPage) * 16;

            //为每个节拍赋值
            for (int i = 0; i < totalBeatsCount ; i++)
            {
                //实例化单个节拍
                Beat beat = new Beat();
                beat.Keys = new List<NoteKey>();
                //单个节拍的开始时间
                beat.Time = singleBeatInterval * i;

                //单个节拍内的按键
                foreach (SongNote note in sheet.songNotes)
                {
                    if(note.time >= beat.Time && note.time < beat.Time + singleBeatInterval)//单个按键时间位于节拍开始与节拍结束之间
                    {
                        //为节拍添加按键
                        NoteKey key = ConvertStringToEnum(note.key);
                        beat.Keys.Add(key);
                    }
                    
                }

                //添加节拍
                song.Beats.Add(beat);
            }

            return song;
        }

        private NoteKey ConvertStringToEnum(string keyName)
        {
            if (keyName.Length == 5)
            {
                keyName = keyName.Insert(4, '0'.ToString());
            }

            string str = "Key" + keyName[0] + keyName.Substring(4);

            return (NoteKey)Enum.Parse(typeof(NoteKey), str);
        }
        #endregion
    }
}
