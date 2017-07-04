using System.Windows;

namespace SimpleAudioPlayer
{
    class ViewModel : BindableBase
    {
        public SettingsModel Setting { get; }
        public PlayerModel Player { get; }

        private bool _IsPlaying;
        public bool IsPlaying { get => _IsPlaying; set => SetProperty(ref _IsPlaying, value); }

        private bool _ShowPlayList;
        public bool ShowPlayList
        {
            get => _ShowPlayList; set
            {
                if(SetProperty(ref _ShowPlayList, value))
                    if(value)
                        playListWindow.Show();
                    else
                        playListWindow.Hide();
            }
        }

        public DelegateCommand CloseCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand OpenPlayListCommand { get; }

        private Window playListWindow;

        public ViewModel()
        {
            playListWindow = new PlayList();
            playListWindow.DataContext = this;

            var serializer = new SerializeHelper<SettingsModel>();
            Setting = serializer.Load();
            Player = new PlayerModel(Setting.PlayList);
            Player.PropertyChanged += Player_PropertyChanged;

            CloseCommand = new DelegateCommand(() => Application.Current.Shutdown());
            SaveCommand = new DelegateCommand(() => serializer.Save(Setting));
            OpenPlayListCommand = new DelegateCommand(() => playListWindow.Show());
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Player.MediaState))
                IsPlaying = Player.MediaState == MediaState.Playing;
        }
    }
}
