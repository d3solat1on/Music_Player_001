using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;
using TagLib;
using NAudio.Flac;
using MusicPlayer_by_d3solat1on.Models;
using MusicPlayer_by_d3solat1on.ViewModels;
using MusicPlayer_by_d3solat1on.Dialogs;
using System.IO;

namespace MusicPlayer_by_d3solat1on.Services
{
    public class PlayerService : IDisposable
    {
        private static PlayerService? _instance;
        public static PlayerService Instance => _instance ??= new PlayerService();

        // NAudio компоненты
        private WaveStream _audioFileReader;
        private WaveOutEvent _waveOutEvent;
        private DispatcherTimer _positionTimer;

        // События
        public event Action<Track> TrackChanged;
        public event Action<double> PositionChanged;
        public event Action<bool> PlaybackPaused;
        public event Action<double> VolumeChanged;
        public event Action DurationChanged;

        // Свойства
        public Track CurrentTrack { get; private set; }
        public bool IsPlaying { get; private set; }

        private double _position;
        public double Position
        {
            get => _position;
            private set
            {
                _position = value;
                PositionChanged?.Invoke(_position);
            }
        }

        private double _duration;
        public double Duration
        {
            get => _duration;
            private set
            {
                if (_duration != value)
                {
                    _duration = value;
                    DurationChanged?.Invoke();
                }
            }
        }

        private double _volume = 0.5;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Max(0, Math.Min(1, value));
                if (_waveOutEvent != null)
                {
                    _waveOutEvent.Volume = (float)_volume;
                }
                VolumeChanged?.Invoke(_volume);
            }
        }

        // Режимы воспроизведения
        private RepeatMode _repeatMode = RepeatMode.NoRepeat;
        public RepeatMode RepeatMode
        {
            get => _repeatMode;
            set
            {
                _repeatMode = value;
                RepeatModeChanged?.Invoke(value);
            }
        }
        public event Action<RepeatMode> RepeatModeChanged;

        private bool _isShuffle = false;
        public bool IsShuffle
        {
            get => _isShuffle;
            set
            {
                _isShuffle = value;
                ShuffleChanged?.Invoke(value);
            }
        }
        public event Action<bool> ShuffleChanged;

        private readonly Random _random = new();

        private PlayerService()
        {
            _positionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _positionTimer.Tick += PositionTimer_Tick;
        }

        public void PlayTrack(Track track)
        {
            try
            {
                Stop();
                CurrentTrack = track;

                // Напрямую открываем файл. MediaFoundationReader не блокируется обложками.
                try
                {
                    _audioFileReader = new AudioFileReader(track.Path);
                }
                catch
                {
                    _audioFileReader = new MediaFoundationReader(track.Path);
                }

                _waveOutEvent = new WaveOutEvent();
                _waveOutEvent.Init(_audioFileReader);
                _waveOutEvent.Play();

                // Обновляем длительность и громкость
                Duration = _audioFileReader.TotalTime.TotalSeconds;
                _waveOutEvent.Volume = (float)_volume;

                IsPlaying = true;
                _positionTimer.Start();

                _waveOutEvent.PlaybackStopped += OnPlaybackStopped;
                LoadCoverForTrack(track);
                TrackChanged?.Invoke(track);
            }
            catch (Exception ex)
            {
                NotificationWindow.Show($"Ошибка: {ex.Message}", Application.Current.MainWindow);
            }
        }
        private static void LoadCoverForTrack(Track track)
        {
            try
            {
                using var file = TagLib.File.Create(track.Path);
                var pic = file.Tag.Pictures.FirstOrDefault();
                if (pic != null)
                {
                    track.CoverImage = pic.Data.Data;
                }
            }
            catch
            {
                track.CoverImage = null;
            }
        }
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // Используем Dispatcher, чтобы корректно вызвать метод из другого потока
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Если трек доиграл до конца (нет ошибки)
                if (e.Exception == null)
                {
                    PlayNextTrack();
                }
            });
        }
        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (_audioFileReader != null && IsPlaying)
            {
                Position = _audioFileReader.CurrentTime.TotalSeconds;
                PositionChanged?.Invoke(Position);

                // Если осталось менее 0.3 сек до конца или текущая позиция превысила длительность
                if (_audioFileReader.CurrentTime >= _audioFileReader.TotalTime - TimeSpan.FromMilliseconds(300))
                {
                    _positionTimer.Stop();
                    PlayNextTrack();
                }
            }
        }

        public void Pause()
        {
            if (_waveOutEvent != null && IsPlaying)
            {
                _waveOutEvent.Pause();
                IsPlaying = false;
                _positionTimer.Stop();
                PlaybackPaused?.Invoke(true);
            }
        }

        public void Resume()
        {
            if (_waveOutEvent != null && !IsPlaying && CurrentTrack != null)
            {
                _waveOutEvent.Play();
                IsPlaying = true;
                _positionTimer.Start();
                PlaybackPaused?.Invoke(false);
            }
        }

        public void Stop()
        {
            _positionTimer?.Stop();

            if (_waveOutEvent != null)
            {
                _waveOutEvent.PlaybackStopped -= OnPlaybackStopped;
                _waveOutEvent.Stop();
                _waveOutEvent.Dispose();
                _waveOutEvent = null;
            }

            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose(); // Это КРИТИЧЕСКИ важно для MediaFoundation
                _audioFileReader = null;
            }

            // Принудительно очищаем ссылки на текущий трек
            CurrentTrack = null;
            IsPlaying = false;
            Position = 0;

            // Вызываем сборщик мусора, чтобы вернуть память системе немедленно
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        public void Seek(double seconds)
        {
            if (_audioFileReader != null && CurrentTrack != null)
            {
                seconds = Math.Max(0, Math.Min(seconds, Duration));
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(seconds);
                Position = seconds;
            }
        }

        private void PlayNextTrack()
        {
            System.Diagnostics.Debug.WriteLine($"=== PlayNextTrack ===");

            var library = MusicLibrary.Instance;
            if (library == null)
            {
                System.Diagnostics.Debug.WriteLine("✗ library == null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"CurrentTracks.Count: {library.CurrentTracks.Count}");
            System.Diagnostics.Debug.WriteLine($"CurrentTrack: {CurrentTrack?.Name}");

            if (library.CurrentTracks.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("✗ Нет треков в текущем плейлисте");
                return;
            }

            var currentIndex = library.CurrentTracks.IndexOf(CurrentTrack);
            System.Diagnostics.Debug.WriteLine($"currentIndex: {currentIndex}");

            // Режим RepeatOne
            if (RepeatMode == RepeatMode.RepeatOne)
            {
                System.Diagnostics.Debug.WriteLine("Режим RepeatOne - повторяем текущий трек");
                PlayTrack(CurrentTrack);
                return;
            }

            // Режим Shuffle
            if (IsShuffle)
            {
                System.Diagnostics.Debug.WriteLine("Режим Shuffle");
                PlayNextShuffleTrack();
                return;
            }

            // Обычный режим
            if (currentIndex >= 0 && currentIndex < library.CurrentTracks.Count - 1)
            {
                var nextTrack = library.CurrentTracks[currentIndex + 1];
                System.Diagnostics.Debug.WriteLine($"Переключаем на следующий трек: {nextTrack.Name}");
                PlayTrack(nextTrack);
            }
            else if (RepeatMode == RepeatMode.RepeatAll && currentIndex == library.CurrentTracks.Count - 1)
            {
                System.Diagnostics.Debug.WriteLine("Режим RepeatAll - начинаем сначала");
                PlayTrack(library.CurrentTracks[0]);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Конец плейлиста, останавливаемся");
            }
        }
        private void PlayNextShuffleTrack()
        {
            var library = MusicLibrary.Instance;
            if (library == null || library.CurrentTracks.Count == 0) return;

            int nextIndex;
            do
            {
                nextIndex = _random.Next(library.CurrentTracks.Count);
            } while (library.CurrentTracks.Count > 1 && nextIndex == library.CurrentTracks.IndexOf(CurrentTrack));

            PlayTrack(library.CurrentTracks[nextIndex]);
        }

        public void Dispose()
        {
            Stop();
            _positionTimer?.Stop();
            _positionTimer = null;
        }
        private static string? CreateFlacWithoutCover(string originalPath)
        {
            try
            {
                // Создаём временный файл
                string tempFile = Path.GetTempFileName() + ".flac";

                // Копируем оригинальный файл
                System.IO.File.Copy(originalPath, tempFile, true);

                // Удаляем обложку из временного файла с помощью TagLib#
                using (var file = TagLib.File.Create(tempFile))
                {
                    // Удаляем все картинки
                    file.Tag.Pictures = [];
                    file.Save();
                }

                System.Diagnostics.Debug.WriteLine($"✓ Создан временный файл без обложки: {tempFile}");
                return tempFile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Ошибка при создании файла без обложки: {ex.Message}");
                return null;
            }
        }
    }

    public enum RepeatMode
    {
        NoRepeat,
        RepeatAll,
        RepeatOne
    }

}