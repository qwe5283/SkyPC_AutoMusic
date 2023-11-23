using MaterialDesignThemes.Wpf;
using SkyPC_AutoMusic.Command;
using SkyPC_AutoMusic.Event;
using SkyPC_AutoMusic.Model;
using SkyPC_AutoMusic.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SkyPC_AutoMusic.ViewModel
{
    internal class TitleBarDetailViewModel : NotificationObject
    {
        private PlayViewModel _playViewModel;

        public string CurrentTime
        {
            get { return _playViewModel.CurrentTime; }
        }

        public string TotalTime
        {
            get { return _playViewModel.TotalTime; }
        }

        public PackIconKind TogglePlayButtonIcon
        {
            get { return _playViewModel.TogglePlayButtonIcon; }
        }

        public String Headline
        {
            get { return _playViewModel.Headline; }
        }

        public DelegateCommand WindowExpandCommand { get; set; }

        public DelegateCommand PreviousSongCommand { get; set; }

        public DelegateCommand NextSongCommand { get; set; }

        public DelegateCommand PauseSongCommand { get; set; }

        public TitleBarDetailViewModel()
        {
            //获取播放器引用
            EA.EventAggregator.GetEvent<GetPlayViewModelEvent>().Subscribe(GetPlayViewModel);//订阅PlayViewModel
            EA.EventAggregator.GetEvent<SynchronizeDetailedTitleBarEvent>().Subscribe(SynchronizeUI);//订阅UI更新
            EA.EventAggregator.GetEvent<RequirePlayViewModelEvent>().Publish();//申请获取PlayViewModel
            //命令
            WindowExpandCommand = new DelegateCommand(WindowExpand);
            NextSongCommand = new DelegateCommand(NextSong);
            PreviousSongCommand = new DelegateCommand(PreviousSong);
            PauseSongCommand = new DelegateCommand(PauseSong);
        }

        private void SynchronizeUI()
        {
            OnPropertyChanged("CurrentTime");
            OnPropertyChanged("TotalTime");
            OnPropertyChanged("TogglePlayButtonIcon");
            OnPropertyChanged("Headline");
        }

        private void GetPlayViewModel(PlayViewModel model)
        {
            _playViewModel = model;
        }

        private void WindowExpand()
        {
            EA.EventAggregator.GetEvent<WindowExpandEvent>().Publish(true);
        }

        private void NextSong()
        {
            EA.EventAggregator.GetEvent<NextPreviousSongEvent>().Publish(true);
        }

        private void PreviousSong()
        {
            EA.EventAggregator.GetEvent<NextPreviousSongEvent>().Publish(false);
        }

        private void PauseSong()
        {
            EA.EventAggregator.GetEvent<PauseSongEvent>().Publish();
            OnPropertyChanged("TogglePlayButtonIcon");
        }
    }
}
