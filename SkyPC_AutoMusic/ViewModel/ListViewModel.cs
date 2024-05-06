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
using System.Threading;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class ListViewModel : NotificationObject
    {
        private string filterText = String.Empty;
        private System.Windows.Controls.ListView listView;

        #region 公开属性

        private ObservableCollection<Song> sheetList;

        public ObservableCollection<Song> SheetList
        {
            get { return sheetList; }
        }

        private Song selectedItem;

        public Song SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; }
        }

        public string HeaderText
        {
            get { return Properties.Resources.List_Header_SheetsList + "(" + SheetList.Count.ToString() + ")"; }
        }

        public DelegateCommand AddSongCommand { get; set; }

        public DelegateCommand DeleteSelectionCommand { get; set; }

        public DelegateCommand SelectFolderCommand { get; set; }

        public DelegateCommand SelectionChangeCommand { get; set; }

        public DelegateCommand FilterDialogCommand { get; set; }
        #endregion

        public ListViewModel(System.Windows.Controls.ListView listView)
        {
            this.listView = listView;
            //私有变量
            EA.EventAggregator.GetEvent<SongSwitchEvent>().Subscribe(SwitchCurrentSong);
            EA.EventAggregator.GetEvent<SelectFolderWithPathEvent>().Subscribe(AutoSelectFolder);
            EA.EventAggregator.GetEvent<SetFilterEvent>().Subscribe(SetFilter);
            //公开属性
            sheetList = new ObservableCollection<Song>();
            //命令
            DeleteSelectionCommand = new DelegateCommand(DeleteSelection);
            SelectFolderCommand = new DelegateCommand(SelectFolder);
            SelectionChangeCommand = new DelegateCommand(SelectionChange);
            AddSongCommand = new DelegateCommand(AddSong);
            FilterDialogCommand = new DelegateCommand(OpenFilterDialog);
            SheetList.CollectionChanged += (sender, e) => { OnPropertyChanged("HeaderText"); };
        }

        #region 私有方法

        private void SetFilter(string filterString)
        {
            filterText = filterString;
            listView.Items.Filter = FilterMethod;
        }

        private bool FilterMethod(object obj)
        {
            var sheet = (Song)obj;

            //return sheet.name.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            return sheet.name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) > -1;
        }

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
            //暂停
            if (PlayViewModel.Instance.IsPlaying())
            {
                EA.EventAggregator.GetEvent<PauseSongEvent>().Publish();
            }
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
                var dialog = SendDialog.WaitTips(Properties.Resources.List_ImportingFiles, totalCount);
            }
            else
            {
                EA.EventAggregator.GetEvent<SendMessageSnackbar>().Publish(String.Format(Properties.Resources.List_ImportingInBackground,totalCount));
            }
            Task.Run(() =>
            {
                //遍历文件
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".txt")
                    {
                        Song song;
                        //从文件实例化类
                        song = ReadJson(file);
                        if (song != null)//成功
                        {
                            //计数
                            successCount++;
                            //添加至列表
                            if (System.Windows.Application.Current != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    SheetList.Add(song);
                                });
                            }
                        }
                        else//失败
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
                    infoFeedback = string.Format(Properties.Resources.List_ImportResult, successCount, failCount);
                    if (showDialog)
                    {
                        if (failCount != 0)
                        {
                            infoFeedback += "\n\n" + Properties.Resources.List_ImportFailureTips + "\n\n" + Properties.Resources.List_ImportFailureList;
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

        private Song ReadJson(string path)
        {
            Sheet sheet;
            Song song;

            try
            {
                // 读取文件内容  
                string rawJson = File.ReadAllText(path);
                // 判断是否加密
                if (rawJson.Contains("\"isEncrypted\":true"))
                {
                    return null;
                }
                // 掐头掐尾
                int startIndex, endIndex;
                startIndex = rawJson.IndexOf('{');
                endIndex = rawJson.LastIndexOf('}');
                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    string json = rawJson.Substring(startIndex, endIndex - startIndex + 1);

                    // 反序列化成C#对象
                    sheet = JsonConvert.DeserializeObject<Sheet>(json);
                    song = ConvertSheetToSong(sheet);

                    return song;
                }
                else
                {
                    // 读不到Json块
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private void AddSong()
        {
            //暂停
            if (PlayViewModel.Instance.IsPlaying())
            {
                EA.EventAggregator.GetEvent<PauseSongEvent>().Publish();
            }
            //打开对话框
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = Properties.Resources.List_ImportWindowTitle;
            dialog.Filter = Properties.Resources.List_TextFile + "(*.txt)|*.txt";
            //反馈信息
            string infoFeedback = string.Empty;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Song song;
                //从文件实例化类
                song = ReadJson(dialog.FileName);
                if (song != null)
                {
                    //添加至列表
                    SheetList.Add(song);
                    infoFeedback = song.name + " " + Properties.Resources.List_ImportSuccessfully;
                }
                else
                {
                    infoFeedback = Properties.Resources.List_ImportFailure + "\n\n" + Properties.Resources.List_ImportFailureTips;
                }
                if (sheetList.Count > 0)
                {
                    //传递乐谱数
                    EA.EventAggregator.GetEvent<PassSheetsCountEvent>().Publish(sheetList.Count);
                    //切歌
                    EA.EventAggregator.GetEvent<FolderSwitchEvent>().Publish();
                }
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
                SendDialog.MessageTips(Properties.Resources.List_NullDelete);
            }
        }

        private void AutoSelectFolder(string folderPath)
        {
            ReadSheetsFromFolder(folderPath, false);
        }

        private Song ConvertSheetToSong(Sheet sheet)
        {
            //检查属性
            if (sheet.bitsPerPage == 0)
            {
                sheet.bitsPerPage = 16;
            }
            if (sheet.bpm == 0)
            {
                sheet.bpm = 240;
            }

            try
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
                int LastBeatsCount = (sheet.songNotes.Max(item => item.time) + singleBeatInterval) / singleBeatInterval;//sheet.songNotes[sheet.songNotes.Count - 1].time / singleBeatInterval;
                int totalBeatsCount = ((LastBeatsCount + song.bitsPerPage - 1) / song.bitsPerPage) * song.bitsPerPage;

                //为每个节拍赋值
                for (int i = 0; i < totalBeatsCount; i++)
                {
                    //实例化单个节拍
                    Beat beat = new Beat();
                    beat.Keys = new List<NoteKey>();
                    //单个节拍的开始时间
                    beat.Time = singleBeatInterval * i;

                    //单个节拍内的按键
                    foreach (SongNote note in sheet.songNotes)
                    {
                        if (note.time >= beat.Time && note.time < beat.Time + singleBeatInterval)//单个按键时间位于节拍开始与节拍结束之间
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private NoteKey ConvertStringToEnum(string keyName)
        {
            string str = keyName.Insert(0, '_'.ToString());
            
            //特判
            if(str == "_1Key15")
            {
                str = "_1Key1";
            }
            else if (str[1] == 'K')
            {
                str = str.Insert(1,'1'.ToString());
            }
            else if (str[1] != '1' && str[1] >= '0' && str[1] <= '9')//键值名为其他数字
            {
                StringBuilder sb = new StringBuilder(str);
                sb[1] = '1';
                str = sb.ToString();
            }

            //将键名转换为枚举
            return (NoteKey)Enum.Parse(typeof(NoteKey), str);
        }

        private void OpenFilterDialog()
        {
            //暂停
            if(PlayViewModel.Instance.IsPlaying())
            {
                EA.EventAggregator.GetEvent<PauseSongEvent>().Publish();
            }
            //显示
            DialogHost.Show(new UserControlFilterDialog(filterText), "RootDialog");
            MainWindow.Instance.Activate();
        }
        #endregion
    }
}
