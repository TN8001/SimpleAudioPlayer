using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SimpleAudioPlayer
{
    [Flags]
    public enum MediaState
    {
        None = 0,
        Ready = 1,
        Loading = 2,
        Loaded = 4,
        Playing = 8,
        Paused = 16,
        Stopped = 32,
        Error = 64,
    }

    class PlayerModel : BindableBase
    {
        private enum RepeatMode { None, Once, All, }

        private PlayItem _CurrentItem;
        public PlayItem CurrentItem { get => _CurrentItem; set => SetProperty(ref _CurrentItem, value); }
        public Duration NaturalDuration => player.NaturalDuration.HasTimeSpan ? player.NaturalDuration : new Duration(TimeSpan.Zero);
        public TimeSpan Position
        {
            get
            {
                return player.Position;
            }
            set
            {
                Debug.WriteLine(value);
                if(value.TotalSeconds < 0) value = TimeSpan.Zero;
                player.Position = value;
                OnPropertyChanged();
            }
        }
        public double Volume { get => player.Volume; set { player.Volume = value; OnPropertyChanged(); } }
        public bool IsMuted { get => player.IsMuted; set { player.IsMuted = value; OnPropertyChanged(); } }
        private bool _IsShuffle;
        public bool IsShuffle { get => _IsShuffle; set { if(SetProperty(ref _IsShuffle, value)) Shuffle(); } }
        private RepeatMode repeatMode;
        public bool IsRepeatOnce
        {
            get => repeatMode == RepeatMode.Once;
            set
            {
                repeatMode = value ? RepeatMode.Once : RepeatMode.None;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRepeatAll));
            }
        }
        public bool IsRepeatAll
        {
            get => repeatMode == RepeatMode.All;
            set
            {
                repeatMode = value ? RepeatMode.All : RepeatMode.None;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRepeatOnce));
            }
        }
        private MediaState _MediaState;
        public MediaState MediaState
        {
            get => _MediaState;
            set
            {
                if(!SetProperty(ref _MediaState, value)) return;
                PlayCommand?.RaiseCanExecuteChanged();
                if(value == MediaState.Ready)
                {
                    OnPropertyChanged(null);
                }
            }
        }

        public DelegateCommand<PlayItem> PlayItemCommand { get; }
        public DelegateCommand PlayCommand { get; }       // ▶️
        public DelegateCommand PauseCommand { get; }      // ⏸
        public DelegateCommand StopCommand { get; }       // ⏹
        public DelegateCommand RewindCommand { get; }     // ⏪
        public DelegateCommand ForwardCommand { get; }    // ⏩
        public DelegateCommand PreviousCommand { get; }   // ⏮
        public DelegateCommand NextCommand { get; }       // ⏭
        public DelegateCommand RepeatAllCommand { get; }  // 🔁
        public DelegateCommand RepeatOnceCommand { get; } // 🔂
        public DelegateCommand ShuffleCommand { get; }    // 🔀
        public DelegateCommand DeleteCommand { get; }


        private MediaPlayer player = new MediaPlayer();
        private PlayItemCollection playList;
        private PlayItemCollection ShuffledList;
        private DispatcherTimer timer;

        public PlayerModel(PlayItemCollection playList)
        {
            this.playList = playList;
            playList.CollectionChanged += (s, e) => Shuffle();

            MediaState = MediaState.Ready;

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => OnPropertyChanged(nameof(Position));

            player.MediaOpened += (s, e) =>
            {
                OnPropertyChanged(nameof(NaturalDuration));
                OnPropertyChanged(nameof(Position));
            };
            player.MediaFailed += (s, e) => MediaState = MediaState.Error;
            player.MediaEnded += (s, e) => { Next(); };

            var canPlay = MediaState.Loaded | MediaState.Paused | MediaState.Stopped;
            var canPause = MediaState.Playing;
            var canStop = MediaState.Playing | MediaState.Paused;
            var canMove = MediaState.Playing | MediaState.Paused;

            PlayItemCommand = new DelegateCommand<PlayItem>((item) => Play(item));
            PlayCommand = new DelegateCommand(() => Play(), () => canPlay.HasFlag(MediaState));
            PauseCommand = new DelegateCommand(() => Pause(), () => canPause.HasFlag(MediaState));
            StopCommand = new DelegateCommand(() => Stop(), () => canStop.HasFlag(MediaState));
            RewindCommand = new DelegateCommand(() => Rewind(), () => canMove.HasFlag(MediaState));
            ForwardCommand = new DelegateCommand(() => Forward(), () => canMove.HasFlag(MediaState));
            PreviousCommand = new DelegateCommand(() => Previous(), () => CurrentItem != null);
            NextCommand = new DelegateCommand(() => Next(), () => CurrentItem != null);
            DeleteCommand = new DelegateCommand(() =>
            {
                var deleteItems = playList.Where(i => i.IsSelected).ToList();
                foreach(var item in deleteItems)
                    playList.Remove(item);
            });
        }

        private void Play(PlayItem item = null)
        {
            if(item != null)
            {
                MediaState = MediaState.Loading;
                player.Open(new Uri(item.FilePath));
                CurrentItem = item;
                MediaState = MediaState.Loaded;
                foreach(var i in playList)
                    i.IsSelected = false;
                item.IsSelected = true;
            }

            if(player.Source == null) return;

            player.Play();
            MediaState = MediaState.Playing;
            timer.Start();
        }
        private void Pause()
        {
            player.Pause();
            MediaState = MediaState.Paused;
            timer.Stop();
        }
        private void Stop()
        {
            player.Stop();
            MediaState = MediaState.Stopped;
            timer.Stop();
            Position = TimeSpan.Zero;
        }
        private void Rewind() => Position -= TimeSpan.FromSeconds(10);
        private void Forward() => Position += TimeSpan.FromSeconds(10);
        private void Previous()
        {
            if(EmptyCheck()) return;

            var list = IsShuffle ? ShuffledList : playList;
            var index = list.IndexOf(CurrentItem);
            switch(repeatMode)
            {
                case RepeatMode.None:
                    if(index > 0) Play(list[index - 1]);
                    break;
                case RepeatMode.Once:
                    Position = TimeSpan.Zero;
                    break;
                case RepeatMode.All:
                    if(index > 0) Play(list[index - 1]);
                    else Play(list.LastOrDefault());
                    break;
            }
        }
        private void Next()
        {
            if(EmptyCheck()) return;

            var list = IsShuffle ? ShuffledList : playList;
            var index = list.IndexOf(CurrentItem);
            switch(repeatMode)
            {
                case RepeatMode.None:
                    if(index < list.Count - 1) Play(list[index + 1]);
                    break;
                case RepeatMode.Once:
                    Position = TimeSpan.Zero;
                    break;
                case RepeatMode.All:
                    if(index < list.Count - 1) Play(list[index + 1]);
                    else Play(list.FirstOrDefault());
                    break;
            }
        }
        private bool EmptyCheck()
        {
            if(playList.Count == 0)
            {
                MediaState = MediaState.Ready;
                CurrentItem = null;
                Position = TimeSpan.Zero;
                player.Close();
                OnPropertyChanged(nameof(NaturalDuration));
            }
            return playList.Count == 0;
        }

        private void Shuffle()
        {
            if(IsShuffle)
                ShuffledList = new PlayItemCollection(playList.OrderBy(i => Guid.NewGuid()).ToList());
        }
    }
}
