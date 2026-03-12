using System;
using System.Windows.Threading;
using System.Windows;
using MusicPlayer_by_d3solat1on.Models;
using MusicPlayer_by_d3solat1on.ViewModels;
using System.Windows.Media;

namespace MusicPlayer_by_d3solat1on.Services
{
    public class PlayerService
    {
        private static PlayerService? _instance;
        public static PlayerService Instance => _instance ??= new PlayerService();

        private readonly DispatcherTimer _positionTimer;

        public event Action<Track> TrackChanged;
        public event Action<double> PositionChanged;
        public event Action<bool> PlaybackPaused;

        private double _volume = 0.5; // Значение по умолчанию 50%

        public Track CurrentTrack { get; private set; }
        public bool IsPlaying { get; private set; }
        public double Position { get; private set; } // в секундах
        public double Duration { get; private set; } // в секундах

        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Max(0, Math.Min(1, value)); // Ограничиваем от 0 до 1
                _mediaPlayer.Volume = _volume;
                VolumeChanged?.Invoke(_volume);
            }
        }

        public event Action<double> VolumeChanged;
        private PlayerService()
        {
            _positionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _positionTimer.Tick += PositionTimer_Tick;
            Volume = 0.5;
        }

        // Для MP3 нам понадобится более мощная библиотека
        // Пока используем MediaPlayer из WPF
        private readonly MediaPlayer _mediaPlayer = new();

        public void PlayTrack(Track track)
        {
            try
            {
                Stop();

                CurrentTrack = track;

                // Сбрасываем длительность до загрузки
                Duration = 0;

                // Подписываемся на событие открытия медиа
                _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

                _mediaPlayer.Open(new Uri(track.Path));
                _mediaPlayer.Play();

                IsPlaying = true;
                _positionTimer.Start();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения: {ex.Message}");
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            // Отписываемся, чтобы не вызвать повторно
            _mediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;

            // Получаем длительность
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Duration = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }

            // Теперь вызываем событие с правильной длительностью
            Application.Current.Dispatcher.Invoke(() =>
            {
                TrackChanged?.Invoke(CurrentTrack);
            });
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            _mediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Ошибка загрузки медиафайла: {e.ErrorException.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        public void Pause()
        {
            if (IsPlaying)
            {
                _mediaPlayer.Pause();
                IsPlaying = false;
                _positionTimer.Stop();
                PlaybackPaused?.Invoke(true);
            }
        }

        public void Resume()
        {
            if (!IsPlaying && CurrentTrack != null)
            {
                _mediaPlayer.Play();
                IsPlaying = true;
                _positionTimer.Start();
                PlaybackPaused?.Invoke(false);
            }
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            IsPlaying = false;
            Position = 0;
            _positionTimer.Stop();
        }

        public void Seek(double seconds)
        {
            if (_mediaPlayer.CanPause && CurrentTrack != null)
            {
                _mediaPlayer.Position = TimeSpan.FromSeconds(seconds);
                Position = seconds;
            }
        }

        private bool _isShuffle = false;
        private Queue<int> _shuffleQueue = new Queue<int>();
        private Random _random = new Random();

        public bool IsShuffle
        {
            get => _isShuffle;
            set
            {
                _isShuffle = value;
                if (_isShuffle)
                {
                    GenerateShuffleQueue();
                }
                ShuffleChanged?.Invoke(value);
            }
        }

        public event Action<bool> ShuffleChanged;

        private void GenerateShuffleQueue()
        {
            var library = MusicLibrary.Instance;
            if (library == null || library.CurrentTracks.Count == 0) return;

            var indices = Enumerable.Range(0, library.CurrentTracks.Count).ToList();
            _shuffleQueue = new Queue<int>(indices.OrderBy(x => _random.Next()));
        }

        public void PlayNextShuffleTrack()
        {
            if (_shuffleQueue.Count == 0)
            {
                GenerateShuffleQueue();
            }

            if (_shuffleQueue.Count > 0)
            {
                var nextIndex = _shuffleQueue.Dequeue();
                var nextTrack = MusicLibrary.Instance.CurrentTracks[nextIndex];
                PlayTrack(nextTrack);
            }
        }
        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (_mediaPlayer.Source != null)
            {
                Position = _mediaPlayer.Position.TotalSeconds;

                if (_mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    Duration = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                }

                PositionChanged?.Invoke(Position);

                if (Duration > 0 && Position >= Duration - 0.5)
                {
                    Stop();

                    var library = MusicLibrary.Instance;
                    if (library == null || library.CurrentTracks.Count == 0) return;

                    var currentIndex = library.CurrentTracks.IndexOf(CurrentTrack);

                    // Режим RepeatOne имеет наивысший приоритет
                    if (RepeatMode1 == RepeatMode.RepeatOne)
                    {
                        PlayTrack(CurrentTrack);
                        return;
                    }

                    // Режим перемешивания
                    if (IsShuffle)
                    {
                        PlayNextShuffleTrack();
                        return;
                    }

                    // Обычный режим
                    switch (RepeatMode1)
                    {
                        case RepeatMode.RepeatAll:
                            if (currentIndex >= 0 && currentIndex < library.CurrentTracks.Count - 1)
                            {
                                PlayTrack(library.CurrentTracks[currentIndex + 1]);
                            }
                            else
                            {
                                PlayTrack(library.CurrentTracks[0]);
                            }
                            break;

                        case RepeatMode.NoRepeat:
                        default:
                            if (currentIndex >= 0 && currentIndex < library.CurrentTracks.Count - 1)
                            {
                                PlayTrack(library.CurrentTracks[currentIndex + 1]);
                            }
                            break;
                    }
                }
            }
        }
        public enum RepeatMode
        {
            NoRepeat,     // Не повторять
            RepeatAll,    // Повторять все
            RepeatOne     // Повторять один трек
        }
        private RepeatMode _repeatMode = RepeatMode.NoRepeat;
        public RepeatMode RepeatMode1
        {
            get => _repeatMode;
            set
            {
                _repeatMode = value;
                RepeatModeChanged?.Invoke(value);
            }
        }

        public event Action<RepeatMode> RepeatModeChanged;


        public void SetVolume(double volume) // от 0 до 1
        {
            _mediaPlayer.Volume = volume;
        }
    }
}