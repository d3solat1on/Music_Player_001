using System;
using System.IO;
using MusicPlayer_by_d3solat1on.Models;
using TagLib;
using File = System.IO.File;

namespace MusicPlayer_by_d3solat1on.Services
{
    public class TagReader
    {
        public static Track? ReadTrackFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            try
            {
                // Просто используем путь - TagLib сам откроет и закроет файл
                using var file = TagLib.File.Create(filePath);

                var track = new Track
                {
                    Path = Path.GetFullPath(filePath),
                    Name = string.IsNullOrEmpty(file.Tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : file.Tag.Title,
                    Executor = file.Tag.FirstPerformer ?? "Неизвестный исполнитель",
                    Album = file.Tag.Album ?? "Неизвестный альбом",
                    Duration = file.Properties.Duration.ToString(@"mm\:ss"),
                    Bitrate = file.Properties.AudioBitrate,
                    SampleRate = file.Properties.AudioSampleRate,
                    Genre = file.Tag.FirstGenre ?? "Неизвестный жанр",
                };
                return track;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения {filePath}: {ex.Message}");
                return new Track { Name = Path.GetFileNameWithoutExtension(filePath), Path = filePath };
            }
        }

        public static Track[] ReadTracksFromFiles(string[] filePaths)
        {
            var tracks = new List<Track>();

            foreach (string filePath in filePaths)
            {
                var track = ReadTrackFromFile(filePath);
                if (track != null)
                    tracks.Add(track);
            }

            return [.. tracks];
        }

        public static Track[] ReadTracksFromFolder(string folderPath, string searchPattern = "*.mp3 | *.flac | *.wav | *.aac")
        {
            var files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);
            return ReadTracksFromFiles(files);
        }

    }
}