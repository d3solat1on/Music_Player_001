using System;
using System.ComponentModel;
using TagLib; // для INotifyPropertyChanged

namespace MusicPlayer_by_d3solat1on.Models
{
    public class Track : INotifyPropertyChanged
    {
        public string? Extension { get; set; }
        public string? Path { get; set; }
        public string? Name { get; set; }
        public string? Executor { get; set; }
        public string? Album { get; set; }
        public string? Duration { get; set; }
        
        public int Bitrate { get; set; }
        public int SampleRate { get; set; }

        // Форматированные свойства для отображения
        public string ExtensionDisplay => !string.IsNullOrEmpty(Extension) ? Extension.ToUpper() : "Неизвестно";
        public string BitrateDisplay => Bitrate > 0 ? $"{Bitrate} kbps" : "Неизвестно";
        public string SampleRateDisplay => SampleRate > 0 ? $"{SampleRate / 1000} kHz" : "Неизвестно";
        public string DurationDisplay => Duration ?? "00:00";
        public string AlbumDisplay => !string.IsNullOrEmpty(Album) ? Album : "Неизвестно";



        // Для обложки альбома (позже добавим)
        public byte[]? CoverImage { get; set; }

        private string _genre;
        public string Genre
        {
            get => _genre;
            set
            {
                _genre = value;
                OnPropertyChanged(nameof(Genre));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}