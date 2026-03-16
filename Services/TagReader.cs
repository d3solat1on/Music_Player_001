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
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var file = TagLib.File.Create(filePath);
                var track = new Track
                {

                    Path = Path.GetFullPath(filePath),
                    Name = string.IsNullOrEmpty(file.Tag.Title) ?
                           Path.GetFileNameWithoutExtension(filePath) :
                           file.Tag.Title,
                    Executor = file.Tag.FirstPerformer ?? "Неизвестный исполнитель",
                    Album = file.Tag.Album ?? "Неизвестный альбом",
                    Duration = file.Properties.Duration.ToString(@"mm\:ss"),
                    Bitrate = file.Properties.AudioBitrate,
                    SampleRate = file.Properties.AudioSampleRate,
                    Genre = file.Tag.FirstGenre ?? "Неизвестный жанр",

                };

                System.Diagnostics.Debug.WriteLine($"=== Загрузка файла: {filePath} ===");
                System.Diagnostics.Debug.WriteLine($"Длительность: {file.Properties.Duration}");
                System.Diagnostics.Debug.WriteLine($"Битрейт: {file.Properties.AudioBitrate}");
                System.Diagnostics.Debug.WriteLine($"Частота: {file.Properties.AudioSampleRate}");
                System.Diagnostics.Debug.WriteLine($"Кодеки: {file.Properties.Description}");
                return track;
            }
            catch (CorruptFileException)
            {
                // Если теги битые, всё равно добавляем трек, используя имя файла
                return new Track
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    Executor = "Corrupt Metadata",
                    Path = Path.GetFullPath(filePath)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Критическая ошибка на файле {filePath}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"=== ОШИБКА ЧТЕНИЯ ТЕГОВ ===");
                System.Diagnostics.Debug.WriteLine($"Файл: {filePath}");
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return null;
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