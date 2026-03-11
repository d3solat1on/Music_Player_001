using System;
using System.IO;
using MusicPlayer_by_d3solat1on.Models;
using TagLib;

namespace MusicPlayer_by_d3solat1on.Services
{
    public class TagReader
    {
        public static Track? ReadTrackFromFile(string filePath)
        {
            try
            {
                using var file = TagLib.File.Create(filePath);
                var track = new Track
                {
                    Path = filePath,
                    Name = string.IsNullOrEmpty(file.Tag.Title) ?
                           Path.GetFileNameWithoutExtension(filePath) :
                           file.Tag.Title,
                    Executor = file.Tag.FirstPerformer ?? "Неизвестный исполнитель",
                    Album = file.Tag.Album ?? "Неизвестный альбом",
                    Duration = file.Properties.Duration.ToString(@"mm\:ss"),
                    Bitrate = file.Properties.AudioBitrate,
                    SampleRate = file.Properties.AudioSampleRate
                };

                // Читаем обложку, если есть
                if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
                {
                    var picture = file.Tag.Pictures[0];
                    track.CoverImage = picture.Data.Data;
                }

                return track;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при чтении {filePath}: {ex.Message}");
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
        
        public static Track[] ReadTracksFromFolder(string folderPath, string searchPattern = "*.mp3")
        {
            var files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);
            return ReadTracksFromFiles(files);
        }
    }
}