using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using QAMP.Dialogs;
using QAMP.Models;
using QAMP.Services;
using QAMP.ViewModels;
using Track = QAMP.Models.Track;

namespace QAMP
{
    public partial class MainWindow
    {
        private static string FormatTime(double seconds)
        {
            if (seconds <= 0) return "0:00";
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1
                ? ts.ToString(@"hh\:mm\:ss")
                : ts.ToString(@"m\:ss");
        }

        private void TogglePlayPause()
        {
            if (Player.CurrentTrack == null)
            {
                if (MusicLibrary.Instance.PlaybackQueue.Count > 0)
                {
                    Player.PlayTrack(MusicLibrary.Instance.PlaybackQueue[0]);
                    UpdateNextTrackUI();
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
            UpdateOSD();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayPause();
        }

        private void UpdatePlayPauseIcon(bool isPlaying)
        {
            PlayPauseIcon.Data = isPlaying
                ? (Geometry)Application.Current.Resources["pauseGeometry"]
                : (Geometry)Application.Current.Resources["playGeometry"];
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playService.IsShuffleEnabled)
            {
                int currentIndex = _playService.ShuffledQueue.IndexOf(Player.CurrentTrack);

                if (currentIndex > 0)
                {
                    var prevTrack = _playService.ShuffledQueue[currentIndex - 1];
                    Player.PlayTrack(prevTrack);
                }
                else if (currentIndex == 0 && _playService.RepeatMode == RepeatMode.RepeatAll)
                {
                    var prevTrack = _playService.ShuffledQueue[_playService.ShuffledQueue.Count - 1];
                    Player.PlayTrack(prevTrack);
                }
                else if (currentIndex == -1)
                {
                    var remainingTracks = MusicLibrary.Instance.PlaybackQueue
                        .Where(t => t != Player.CurrentTrack)
                        .OrderBy(x => Guid.NewGuid())
                        .ToList();

                    _playService.ShuffledQueue = [Player.CurrentTrack, ..remainingTracks];

                    if (_playService.RepeatMode == RepeatMode.RepeatAll && _playService.ShuffledQueue.Count > 1)
                    {
                        var prevTrack = _playService.ShuffledQueue[_playService.ShuffledQueue.Count - 1];
                        Player.PlayTrack(prevTrack);
                    }
                }

                UpdateNextTrackUI();
            }
            else
            {
                var currentIndex = MusicLibrary.Instance.PlaybackQueue.IndexOf(Player.CurrentTrack);
                if (currentIndex > 0)
                {
                    Player.PlayTrack(MusicLibrary.Instance.PlaybackQueue[currentIndex - 1]);
                    UpdateNextTrackUI();
                }
                else if (_playService.RepeatMode == RepeatMode.RepeatAll && MusicLibrary.Instance.PlaybackQueue.Count > 0)
                {
                    Player.PlayTrack(MusicLibrary.Instance.PlaybackQueue[MusicLibrary.Instance.PlaybackQueue.Count - 1]);
                    UpdateNextTrackUI();
                }
                else
                {
                    NotificationWindow.Show("Это первый трек в плейлисте", this);
                }
            }
            if (_isLyricsMode)
            {
                UpdateLyricsView();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playService.IsShuffleEnabled)
            {
                int currentIndex = _playService.ShuffledQueue.IndexOf(Player.CurrentTrack);

                if (currentIndex != -1 && currentIndex < _playService.ShuffledQueue.Count - 1)
                {
                    var nextTrack = _playService.ShuffledQueue[currentIndex + 1];
                    Player.PlayTrack(nextTrack);
                }
                else if (currentIndex == _playService.ShuffledQueue.Count - 1)
                {
                    if (_playService.RepeatMode == RepeatMode.RepeatAll)
                    {
                        var nextTrack = _playService.ShuffledQueue[0];
                        Player.PlayTrack(nextTrack);
                    }
                    else
                    {
                        NotificationWindow.Show("Плейлист закончился", this);
                    }
                }
                else
                {
                    var remainingTracks = MusicLibrary.Instance.PlaybackQueue
                        .Where(t => t != Player.CurrentTrack)
                        .OrderBy(x => Guid.NewGuid())
                        .ToList();

                    _playService.ShuffledQueue = [Player.CurrentTrack, ..remainingTracks];
                    if (_playService.ShuffledQueue.Count > 1)
                    {
                        var nextTrack = _playService.ShuffledQueue[1];
                        Player.PlayTrack(nextTrack);
                    }
                }

                UpdateNextTrackUI();
            }
            else
            {
                var currentIndex = MusicLibrary.Instance.PlaybackQueue.IndexOf(Player.CurrentTrack);
                if (currentIndex < MusicLibrary.Instance.PlaybackQueue.Count - 1)
                {
                    Player.PlayTrack(MusicLibrary.Instance.PlaybackQueue[currentIndex + 1]);
                    UpdateNextTrackUI();
                }
                else if (_playService.RepeatMode == RepeatMode.RepeatAll && MusicLibrary.Instance.PlaybackQueue.Count > 0)
                {
                    Player.PlayTrack(MusicLibrary.Instance.PlaybackQueue[0]);
                    UpdateNextTrackUI();
                }
                else
                {
                    NotificationWindow.Show("Плейлист закончился", this);
                }
            }
            if (_isLyricsMode)
            {
                UpdateLyricsView();
            }
        }

        private void UpdateNextTrackUI(Track? currentTrack = null)
        {
            var next = _playService.GetNextTrack();

            if (next != null)
            {
                NextTrackName.Text = $"{next.Executor} - {next.Name}";
                NextTrackName.Foreground = Brushes.White;
            }
            else
            {
                NextTrackName.Text = "Плейлист закончился";
                NextTrackName.Foreground = Brushes.DimGray;
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            _playService.IsShuffleEnabled = !_playService.IsShuffleEnabled;

            if (_playService.IsShuffleEnabled)
            {
                var shuffledList = MusicLibrary.Instance.PlaybackQueue.ToList();

                Track? currentTrack = Player.CurrentTrack;
                if (currentTrack != null && shuffledList.Contains(currentTrack))
                {
                    shuffledList.Remove(currentTrack);
                }

                shuffledList = [..shuffledList.OrderBy(x => Guid.NewGuid())];

                if (currentTrack != null)
                {
                    shuffledList.Insert(0, currentTrack);
                }

                _playService.ShuffledQueue = shuffledList;

                ShuffleImage.Data = (Geometry)Application.Current.Resources["shuffle_OnGeometry"];
                ShuffleImage.Fill = (Brush)Application.Current.Resources["AccentBrush"];
                UpdateNextTrackUI();
            }
            else
            {
                _playService.ShuffledQueue.Clear();
                ShuffleImage.Data = (Geometry)Application.Current.Resources["shuffleGeometry"];
                ShuffleImage.Fill = (Brush)Application.Current.Resources["AccentBrush"];
                UpdateNextTrackUI();
            }
        }

        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isSliderDragging = true;
        }

        private void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isSliderDragging = false;
            if (Player.CurrentTrack != null)
            {
                var newPosition = ProgressSlider.Value / 100 * Player.Duration;
                Player.Seek(newPosition);
            }
        }

        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Slider slider) return;

            Point point = e.GetPosition(slider);
            double relativePosition = point.X / slider.ActualWidth;
            double newValue = slider.Minimum + (relativePosition * (slider.Maximum - slider.Minimum));
            slider.Value = newValue;
        }
    }
}
