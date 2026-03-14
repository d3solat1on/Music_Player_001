using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using MusicPlayer_by_d3solat1on.Models;
using MusicPlayer_by_d3solat1on.Services;

namespace MusicPlayer_by_d3solat1on.ViewModels
{
    public class MusicLibrary : INotifyPropertyChanged
    {
        private static MusicLibrary? _instance;
        public static MusicLibrary Instance => _instance ??= new MusicLibrary();

        // Все треки
        public ObservableCollection<Track> AllTracks { get; set; } = [];

        // Все плейлисты
        public ObservableCollection<Playlist> Playlists { get; set; } = [];
        public const string FavoritesName = "Избранное";

        private MusicLibrary()
        {
            if (Playlists == null) Playlists = [];
            if (!Playlists.Any(p => p.Name == FavoritesName))
            {
                // Сначала получаем данные картинки из ресурсов
                byte[]? defaultImageData = null;
                try
                {
                    var uri = new Uri("pack://application:,,,/Resources/favorites_cover.png");
                    var info = Application.GetResourceStream(uri);
                    using var ms = new System.IO.MemoryStream();
                    info.Stream.CopyTo(ms);
                    defaultImageData = ms.ToArray();
                }
                catch { /* Если файл не найден, останется null */ }

                Playlists.Add(new Playlist
                {
                    Id = 0,
                    Name = FavoritesName,
                    Description = "Ваши любимые треки",
                    CreatedDate = DateTime.Now,
                    CoverImage = defaultImageData,
                    Tracks = [] 
                });
            }
        }

        // Текущий выбранный плейлист
        private Playlist? _currentPlaylist;
        public Playlist CurrentPlaylist
        {
            get => _currentPlaylist;
            set
            {
                _currentPlaylist = value;
                OnPropertyChanged(nameof(CurrentPlaylist));
                OnPropertyChanged(nameof(CurrentTracks)); // обновляем список треков
            }
        }

        // Треки текущего плейлиста (или все треки, если плейлист не выбран)
        public ObservableCollection<Track> CurrentTracks =>
            CurrentPlaylist != null ? CurrentPlaylist.Tracks : AllTracks;

        // Последний проигранный трек
        private Track? _lastPlayedTrack;
        public Track LastPlayedTrack
        {
            get => _lastPlayedTrack;
            set
            {
                _lastPlayedTrack = value;
                OnPropertyChanged(nameof(LastPlayedTrack));
            }
        }

        // Текущий проигрываемый трек
        private Track? _currentTrack;
        public Track CurrentTrack
        {
            get => _currentTrack;
            set
            {
                _currentTrack = value;
                OnPropertyChanged(nameof(CurrentTrack));
            }
        }

        public void AddTracks(string[] filePaths)
        {
            var tracks = TagReader.ReadTracksFromFiles(filePaths);
            foreach (var track in tracks)
            {
                if (track != null)
                    AllTracks.Add(track);
            }
        }

        public void AddTracksFromFolder(string folderPath)
        {
            var tracks = TagReader.ReadTracksFromFolder(folderPath);
            foreach (var track in tracks)
            {
                if (track != null)
                    AllTracks.Add(track);
            }
        }

        public Playlist CreatePlaylist(string name, string description = "", byte[]? coverImage = null)
        {
            var playlist = new Playlist
            {
                Id = Playlists.Count + 1,
                Name = name,
                Description = description,
                CoverImage = coverImage,
                CreatedDate = DateTime.Now
            };

            Playlists.Add(playlist);
            return playlist;
        }

        public static void AddTrackToPlaylist(Playlist playlist, Track track)
        {
            if (!playlist.Tracks.Contains(track))
                playlist.Tracks.Add(track);
        }

        public static void RemoveTrackFromPlaylist(Playlist playlist, Track track)
        {
            playlist.Tracks.Remove(track);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void AddTracksToCurrentPlaylist(string[] filePaths)
        {
            if (CurrentPlaylist == null)
            {
                System.Windows.MessageBox.Show("Сначала выберите плейлист", "Информация",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var tracks = Services.TagReader.ReadTracksFromFiles(filePaths);
            foreach (var track in tracks)
            {
                if (track != null)
                {
                    // Добавляем в общую библиотеку, если там ещё нет
                    if (!AllTracks.Any(t => t.Path == track.Path))
                    {
                        AllTracks.Add(track);
                    }
                    // Добавляем в текущий плейлист
                    CurrentPlaylist.Tracks.Add(track);
                }
            }

            // Обновляем отображение
            OnPropertyChanged(nameof(CurrentTracks));
        }

        public void AddTracksFromFolderToCurrentPlaylist(string folderPath)
        {
            if (CurrentPlaylist == null)
            {
                System.Windows.MessageBox.Show("Сначала выберите плейлист", "Информация",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var tracks = Services.TagReader.ReadTracksFromFolder(folderPath);
            foreach (var track in tracks)
            {
                if (track != null)
                {
                    if (!AllTracks.Any(t => t.Path == track.Path))
                    {
                        AllTracks.Add(track);
                    }
                    CurrentPlaylist.Tracks.Add(track);
                }
            }

            OnPropertyChanged(nameof(CurrentTracks));
        }
        public void UpdatePlaylistView()
        {
            OnPropertyChanged(nameof(CurrentPlaylist));
            OnPropertyChanged(nameof(CurrentTracks));
        }
    }
}