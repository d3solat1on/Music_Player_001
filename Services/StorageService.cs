using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using MusicPlayer_by_d3solat1on.Models;
using MusicPlayer_by_d3solat1on.ViewModels;

namespace MusicPlayer_by_d3solat1on.Services
{
    public class StorageService
    {
        private static StorageService? _instance;
        public static StorageService Instance => _instance ??= new StorageService();

        private readonly string _appDataPath;
        private readonly string _libraryFilePath;
        public double Volume { get; set; } = 0.5;

        private StorageService()
        {
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MusicPlayer_by_d3solat1on");

            if (!Directory.Exists(_appDataPath))
                Directory.CreateDirectory(_appDataPath);

            _libraryFilePath = Path.Combine(_appDataPath, "library.json");
        }

        // Сохраняем библиотеку
        public void SaveLibrary()
        {
            try
            {
                var data = new LibraryData
                {
                    Tracks = [.. MusicLibrary.Instance.AllTracks],
                    Playlists = [.. MusicLibrary.Instance.Playlists]
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                File.WriteAllText(_libraryFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        // Загружаем библиотеку
        public void LoadLibrary()
        {
            try
            {
                if (File.Exists(_libraryFilePath))
                {
                    var json = File.ReadAllText(_libraryFilePath);
                    var data = JsonConvert.DeserializeObject<LibraryData>(json,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });

                    if (data != null)
                    {
                        MusicLibrary.Instance.AllTracks.Clear();
                        foreach (var track in data.Tracks)
                            MusicLibrary.Instance.AllTracks.Add(track);

                        MusicLibrary.Instance.Playlists.Clear();
                        foreach (var playlist in data.Playlists)
                            MusicLibrary.Instance.Playlists.Add(playlist);
                        PlayerService.Instance.Volume = data.Volume;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки: {ex.Message}");
            }
        }

        [Serializable]
        private class LibraryData
        {
            public List<Track>? Tracks { get; set; }
            public List<Playlist>? Playlists { get; set; }
            public double Volume { get; set; } = 0.5;
        }
    }
}