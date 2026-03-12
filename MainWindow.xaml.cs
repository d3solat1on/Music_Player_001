using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MusicPlayer_by_d3solat1on.Dialogs;
using MusicPlayer_by_d3solat1on.Models;
using MusicPlayer_by_d3solat1on.Services;
using MusicPlayer_by_d3solat1on.ViewModels;
using static MusicPlayer_by_d3solat1on.Services.PlayerService;
using Track = MusicPlayer_by_d3solat1on.Models.Track;

namespace MusicPlayer_by_d3solat1on
{
    public partial class MainWindow : Window
    {
        public static MusicLibrary Library => MusicLibrary.Instance;
        private static PlayerService Player => Instance;
        private bool _isSliderDragging = false;


        public MainWindow()
        {
            InitializeComponent();

            TracksDataGrid.ItemsSource = Library.CurrentTracks;
            PlaylistsListBox.ItemsSource = Library.Playlists;


            Player.TrackChanged += OnTrackChanged;
            Player.PositionChanged += OnPositionChanged;
            Player.PlaybackPaused += OnPlaybackPaused;
            Player.VolumeChanged += OnVolumeChanged;

            VolumeSlider.Value = Player.Volume * 100;

            Library.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MusicLibrary.CurrentTracks))
                    TracksDataGrid.ItemsSource = Library.CurrentTracks;
            };
            if (VolumePercentage != null)
            {
                VolumePercentage.Text = $"{VolumeSlider.Value:F0}%";
            }

            StorageService.Instance.LoadLibrary();


            Closed += (s, e) => StorageService.Instance.SaveLibrary();
            VolumeSlider.Value = 50;
        }

        private void OnVolumeChanged(double volume)
        {
            Dispatcher.Invoke(() =>
            {
                VolumeSlider.Value = volume * 100;
                VolumePercentage.Text = $"{volume * 100:F0}%";

            });
        }
        private void OnTrackChanged(Track track)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateNowPlayingInfo(track);
                UpdatePlayPauseIcon(true);

                // Обновляем длительность
                string totalTime = "0:00";
                if (Player.Duration > 0)
                {
                    totalTime = FormatTime(Player.Duration);
                }
                else
                {
                    // Если длительность еще не загружена, показываем "загрузка"
                    totalTime = "загрузка...";

                    // Планируем повторную проверку через небольшие интервалы
                    CheckDurationAsync();
                }

                TotalTimeText.Text = totalTime;
            });
        }
        private async void CheckDurationAsync()
        {
            // Проверяем длительность несколько раз с задержкой
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);

                if (Player.Duration > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        TotalTimeText.Text = FormatTime(Player.Duration);
                    });
                    break;
                }
            }
        }
        private void OnPositionChanged(double position)
        {
            Dispatcher.Invoke(() =>
            {
                if (!_isSliderDragging)
                {

                    if (Player.Duration > 0)
                    {
                        ProgressSlider.Value = position / Player.Duration * 100;
                    }
                    CurrentTimeText.Text = FormatTime(position);
                }
            });
        }

        private void OnPlaybackPaused(bool isPaused)
        {
            Dispatcher.Invoke(() =>
            {
                UpdatePlayPauseIcon(!isPaused);
            });
        }



        private static string FormatTime(double seconds)
        {
            if (seconds <= 0) return "0:00";
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1
                ? ts.ToString(@"hh\:mm\:ss")
                : ts.ToString(@"m\:ss");
        }


        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.CurrentTrack == null)
            {

                if (Library.CurrentTracks.Count > 0)
                {
                    Player.PlayTrack(Library.CurrentTracks[0]);
                }
            }
            else if (Player.IsPlaying)
            {
                Player.Pause();
            }
            else
            {
                Player.Resume();
            }
        }

        private void UpdatePlayPauseIcon(bool isPlaying)
        {
            if (PlayPauseButton.Content is Image image)
            {
                if (isPlaying)
                {
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/pause.png"));
                }
                else
                {
                    image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/play.png"));
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
            PlayPauseButton.Content = "▶";
            ProgressSlider.Value = 0;
            CurrentTimeText.Text = "0:00";
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {

            if (Library.CurrentTracks.Count == 0) return;

            var currentIndex = Library.CurrentTracks.IndexOf(Player.CurrentTrack);
            if (currentIndex > 0)
            {
                Player.PlayTrack(Library.CurrentTracks[currentIndex - 1]);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {

            if (Library.CurrentTracks.Count == 0) return;

            var currentIndex = Library.CurrentTracks.IndexOf(Player.CurrentTrack);
            if (currentIndex < Library.CurrentTracks.Count - 1)
            {
                Player.PlayTrack(Library.CurrentTracks[currentIndex + 1]);
            }
        }


        private void ProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isSliderDragging = true;
        }

        private void ProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isSliderDragging = false;
            if (Player.CurrentTrack != null)
            {
                var newPosition = ProgressSlider.Value / 100 * Player.Duration;
                Player.Seek(newPosition);
            }
        }


        private void AddMusicButton_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();

            if (Library.CurrentPlaylist != null)
            {

                var addFilesToPlaylistItem = new MenuItem
                {
                    Header = $"Добавить файлы в плейлист \"{Library.CurrentPlaylist.Name}\"..."
                };
                addFilesToPlaylistItem.Click += (s, args) => AddFilesToCurrentPlaylist();
                contextMenu.Items.Add(addFilesToPlaylistItem);

                var addFolderToPlaylistItem = new MenuItem
                {
                    Header = $"Добавить папку в плейлист \"{Library.CurrentPlaylist.Name}\"..."
                };
                addFolderToPlaylistItem.Click += (s, args) => AddFolderToCurrentPlaylist();
                contextMenu.Items.Add(addFolderToPlaylistItem);

                contextMenu.Items.Add(new Separator());
            }


            var addFilesItem = new MenuItem { Header = "Добавить файлы в библиотеку..." };
            addFilesItem.Click += (s, args) => AddFiles();
            contextMenu.Items.Add(addFilesItem);

            var addFolderItem = new MenuItem { Header = "Добавить папку в библиотеку..." };
            addFolderItem.Click += (s, args) => AddFolder();
            contextMenu.Items.Add(addFolderItem);

            contextMenu.PlacementTarget = sender as UIElement;
            contextMenu.IsOpen = true;
        }
        private static void AddFilesToCurrentPlaylist()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите музыкальные файлы для плейлиста",
                Filter = "Музыкальные файлы|*.mp3;*.wav;*.flac;*.m4a|Все файлы|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Library.AddTracksToCurrentPlaylist(openFileDialog.FileNames);
            }
        }

        private static void AddFolderToCurrentPlaylist()
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Выберите папку с музыкой для плейлиста"
            };

            if (folderDialog.ShowDialog() == true)
            {
                Library.AddTracksFromFolderToCurrentPlaylist(folderDialog.FolderName);
            }
        }

        private static void AddFiles()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите музыкальные файлы",
                Filter = "Музыкальные файлы|*.mp3;*.wav;*.flac;*.m4a|Все файлы|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Library.AddTracks(openFileDialog.FileNames);
            }
        }

        private static void AddFolder()
        {
            var folderDialog = new OpenFolderDialog
            {
                Title = "Выберите папку с музыкой"
            };

            if (folderDialog.ShowDialog() == true)
            {
                Library.AddTracksFromFolder(folderDialog.FolderName);
            }
        }


        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreatePlaylistDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                Library.CreatePlaylist(dialog.PlaylistName, dialog.PlaylistDescription, dialog.PlaylistCoverImage);
            }
        }


        private void PlaylistsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistsListBox.SelectedItem is Playlist selectedPlaylist)
            {
                Library.CurrentPlaylist = selectedPlaylist;
            }
        }


        private void TracksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TracksDataGrid.SelectedItem is Track selectedTrack)
            {
                Player.PlayTrack(selectedTrack);
            }
        }

        private void UpdateNowPlayingInfo(Track track)
        {

    
            LastTrackName.Text = track.Name;
            LastTrackExecutor.Text = track.Executor;
            LastTrackAlbum.Text = track.Album;
            LastTrackData.Text = $"{track.ExtensionDisplay}, {track.SampleRateDisplay}, {track.BitrateDisplay}, JOINT STEREO";

            CurrentTrackName.Text = track.Name;
            CurrentTrackExecutor.Text = track.Executor;
            CurrentTrackAlbum.Text = track.Album;
            CurrentTrackData.Text = $"{track.ExtensionDisplay}, {track.SampleRateDisplay}, {track.BitrateDisplay}, {track.AlbumDisplay}, {track.Genre} JOINT STEREO";
           

            Library.LastPlayedTrack = track;
        }

        private void RemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (Library.CurrentPlaylist == null)
            {
                MessageBox.Show("Сначала выберите плейлист", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (TracksDataGrid.SelectedItem is Track selectedTrack)
            {
                var result = MessageBox.Show(
                    $"Удалить трек \"{selectedTrack.Name}\" из плейлиста \"{Library.CurrentPlaylist.Name}\"?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Library.CurrentPlaylist.Tracks.Remove(selectedTrack);
                }
            }
        }

        private void RemoveFromLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (TracksDataGrid.SelectedItem is Track selectedTrack)
            {
                var result = MessageBox.Show(
                    $"Удалить трек \"{selectedTrack.Name}\" из библиотеки?\n" +
                    "Это удалит его из всех плейлистов!",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {

                    foreach (var playlist in Library.Playlists)
                    {
                        playlist.Tracks.Remove(selectedTrack);
                    }


                    Library.AllTracks.Remove(selectedTrack);
                }
            }
        }
        private void RemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistsListBox.SelectedItem is Playlist selectedPlaylist)
            {
                var result = MessageBox.Show(
                    $"Удалить плейлист \"{selectedPlaylist.Name}\"?\n" +
                    "Треки в плейлисте останутся в библиотеке.",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Library.Playlists.Remove(selectedPlaylist);
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Теперь все элементы точно загружены
            if (VolumePercentage != null)
            {
                VolumePercentage.Text = $"{VolumeSlider.Value:F0}%";
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Player != null)
            {
                double volume = VolumeSlider.Value / 100.0;
                Player.Volume = volume;

                // Проверка на null теперь работает, так как элемент загружен
                if (VolumePercentage != null)
                {
                    VolumePercentage.Text = $"{VolumeSlider.Value:F0}%";
                }
            }
        }
        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            Player.IsShuffle = !Player.IsShuffle;
            ShuffleButton.Background = Player.IsShuffle ?
                new SolidColorBrush(Color.FromRgb(0, 122, 204)) :
                new SolidColorBrush(Color.FromRgb(64, 64, 64));
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            var RepeatButton = new RepeatButton();
            switch (Player.RepeatMode1)
            {
                case RepeatMode.NoRepeat:
                    Player.RepeatMode1 = RepeatMode.RepeatAll;
                    RepeatButton.Content = "🔁";
                    break;
                case RepeatMode.RepeatAll:
                    Player.RepeatMode1 = RepeatMode.RepeatOne;
                    RepeatButton.Content = "🔂";
                    break;
                case RepeatMode.RepeatOne:
                    Player.RepeatMode1 = RepeatMode.NoRepeat;
                    RepeatButton.Content = "➡️";
                    break;
            }
        }

    }
}