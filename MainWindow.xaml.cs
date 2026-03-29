using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using QAMP.Audio;
using QAMP.Dialogs;
using QAMP.Models;
using QAMP.Services;
using QAMP.ViewModels;

namespace QAMP
{
    public partial class MainWindow : Window
    {
        private readonly PlayerService _playService = PlayerService.Instance;
        private readonly DispatcherTimer _memoryCleanupTimer;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        public static MusicLibrary Library => MusicLibrary.Instance;
        private static PlayerService Player => PlayerService.Instance;
        private bool _isSliderDragging = false;
        private double _lastVolume = 0.5;
        private Track? _lastTrackWithCover;
        private bool _isLyricsMode = false;

        public MainWindow()
        {
            InitializeComponent();
            SetupTrayIcon();

            System.Diagnostics.Debug.WriteLine("=== ПУТЬ К БАЗЕ ДАННЫХ ===");
            System.Diagnostics.Debug.WriteLine($"Путь: {DatabaseService.DatabasePath}");
            System.Diagnostics.Debug.WriteLine($"Папка существует: {Directory.Exists(Path.GetDirectoryName(DatabaseService.DatabasePath))}");

            DatabaseService.EnsureDatabaseCreated();

            System.Diagnostics.Debug.WriteLine($"База данных существует: {File.Exists(DatabaseService.DatabasePath)}");
            LoadPlaylists();
            LoadApplicationSettings();
            DataContext = MusicLibrary.Instance;

            Player.TrackChanged += OnTrackChanged;
            Player.PositionChanged += OnPositionChanged;
            Player.PlaybackPaused += OnPlaybackPaused;
            Player.VolumeChanged += OnVolumeChanged;
            Player.DurationChanged += OnDurationChanged;
            _playService.TrackChanged += UpdateNextTrackUI;
            PlaylistsListBox.MouseDoubleClick += PlaylistsListBox_MouseDoubleClick;
            PreviewKeyDown += Window_KeyDown;

            _memoryCleanupTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5)
            };
            _memoryCleanupTimer.Tick += (s, e) =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            };
            _memoryCleanupTimer.Start();

            Closed += (s, e) => OnClosing((CancelEventArgs)e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (VolumePercentage != null)
            {
                VolumePercentage.Text = $"{VolumeSlider.Value:F0}%";
            }
        }

        private void LoadApplicationSettings()
        {
            ThemeManager.ApplyTheme(SettingsManager.Instance.Config.ColorScheme);

            string savedVolume = DatabaseService.GetSetting("Volume", "0.5");
            if (double.TryParse(savedVolume, out double vol))
            {
                _playService.Volume = vol;
                VolumeSlider.Value = vol * 100;
            }

            string lastPlaylistId = DatabaseService.GetSetting("LastPlaylistId", "-1");
            if (int.TryParse(lastPlaylistId, out int id) && id != -1)
            {
                var playlist = MusicLibrary.Instance.Playlists.FirstOrDefault(p => p.Id == id);
                if (playlist != null)
                {
                    PlaylistsListBox.SelectedItem = playlist;
                }
            }
        }

        private void OnDurationChanged()
        {
            Dispatcher.Invoke(() => { TotalTimeText.Text = FormatTime(Player.Duration); });
        }

        private void OnVolumeChanged(double volume)
        {
            Dispatcher.Invoke(() =>
            {
                VolumeSlider.Value = volume * 100;
                VolumePercentage.Text = $"{volume * 100:F0}%";
            });
        }

        private void OnTrackChanged(Track? track)
        {
            if (track == null) return;
            Dispatcher.Invoke(() =>
            {
                UpdateNowPlayingInfo(track);
                UpdatePlayPauseIcon(true);
                UpdateFavoriteIcon(track);
                CurrentTrackName.Text = track.Name;
                CurrentTrackExecutor.Text = track.Executor;
                CurrentTrackAlbum.Text = track.Album;
                CurrentTrackData.Text = $"{track.Genre} | {track.Duration} | {track.SampleRate} Hz | {track.Bitrate} kbps";
                CurrentTrackExtension.Text = track.Extension;
                CurrentTrackYear.Text = track.Year > 0 ? track.Year.ToString() : "Неизвестно";

                string totalTime = Player.Duration > 0 ? FormatTime(Player.Duration) : "Загрузка...";
                if (Player.Duration <= 0) CheckDurationAsync();

                UpdateCurrentTrackCover(track);
                TotalTimeText.Text = totalTime;
            });
        }

        private async void CheckDurationAsync()
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                if (Player.Duration > 0)
                {
                    Dispatcher.Invoke(() => { TotalTimeText.Text = FormatTime(Player.Duration); });
                    break;
                }
            }
        }

        private void OnPositionChanged(double position)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnPositionChanged(position));
                return;
            }

            if (!_isSliderDragging)
            {
                if (Player.Duration > 0)
                {
                    double sliderValue = position / Player.Duration * 100;
                    if (!double.IsNaN(sliderValue) && !double.IsInfinity(sliderValue))
                    {
                        ProgressSlider.Value = sliderValue;
                    }
                }
                CurrentTimeText.Text = FormatTime(position);
            }
        }

        private void OnPlaybackPaused(bool isPaused)
        {
            Dispatcher.Invoke(() => { UpdatePlayPauseIcon(!isPaused); });
        }
    }
}
