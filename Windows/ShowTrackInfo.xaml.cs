using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TagLib;
using QAMP.Dialogs;
using QAMP.Models;
using Microsoft.Win32;

namespace QAMP.Windows
{
    public partial class ShowTrackInfo : Window
    {
        private readonly Track _track;
        public ShowTrackInfo(Track track)
        {
            InitializeComponent();
            _track = track;
            DataContext = track;
            Loaded += ShowTrackInfo_Loaded;
        }

        private void ShowTrackInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (FindName("PathTextBlock") is System.Windows.Controls.TextBlock pathTextBlock)
            {
                pathTextBlock.MouseLeftButtonDown += PathTextBlock_MouseLeftButtonDown;
            }
        }

        private void PathTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(_track?.Path)) return;

            try
            {
                // Получаем директорию файла
                string directory = Path.GetDirectoryName(_track.Path);

                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    Process.Start("explorer.exe", $"/select,\"{_track.Path}\"");
                }
                else
                {
                    NotificationWindow.Show("Папка с файлом не найдена", this);
                }
            }
            catch (Exception ex)
            {
                NotificationWindow.Show($"Ошибка открытия папки: {ex.Message}", this);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var file = TagLib.File.Create(_track.Path))
                {
                    file.Tag.Title = _track.Name;
                    file.Tag.Performers = [_track.Executor];
                    file.Tag.Album = _track.Album;
                    file.Tag.AlbumArtists = [_track.AlbumArtist];
                    file.Tag.Genres = [_track.Genre];
                    file.Tag.Comment = _track.Comment;
                    file.Tag.Lyrics = _track.Lyrics;
                    file.Tag.Composers = [_track.Composer];

                    // if (uint.TryParse(_track.TrackNumber.ToString(), out uint trackNum))
                    //     file.Tag.TrackNumber = trackNum;

                    // if (uint.TryParse(_track.Bpm.ToString(), out uint bpm))
                    //     file.Tag.BeatsPerMinute = bpm;

                    if (uint.TryParse(_track.Year.ToString(), out uint year))
                        file.Tag.Year = year;

                    file.Save();
                }

                NotificationWindow.Show("Теги сохранены!", this);

                EditModeButton.IsChecked = false;
            }
            catch (Exception ex)
            {
                NotificationWindow.Show($"Ошибка: {ex.Message}", this);
            }
        }

        private void ChangeCover_Click(object sender, MouseButtonEventArgs e)
        {
            if (EditModeButton.IsChecked != true) return;

            OpenFileDialog ofd = new()
            {
                Filter = "Images|*.jpg;*.jpeg;*.png"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    using (var file = TagLib.File.Create(_track.Path))
                    {
                        var picture = new Picture(ofd.FileName);
                        file.Tag.Pictures = [picture];
                        file.Save();
                    }

                    _track.CoverImage = System.IO.File.ReadAllBytes(ofd.FileName); 
                    NotificationWindow.Show("Обложка обновлена! Перезапустите трек.", this);
                }
                catch (Exception ex)
                {
                    NotificationWindow.Show($"Ошибка: {ex.Message}", this);
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}