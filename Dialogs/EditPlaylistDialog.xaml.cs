using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QAMP.Models;
using QAMP.ViewModels;
namespace QAMP.Dialogs
{

    public partial class EditPlaylistDialog : Window
    {

        public EditPlaylistDialog()
        {
            InitializeComponent();
        }
        private void SelectCoverButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 1. Открываем окно обрезки
                var cropper = new ImageCropperDialog(openFileDialog.FileName)
                {
                    Owner = GetWindow(this)
                };

                if (cropper.ShowDialog() == true && cropper.ResultImage != null)
                {
                    // 2. Отображаем результат в UI (превью в круге/квадрате)
                    CoverImage.Source = cropper.ResultImage;
                    if (PlaceholderText != null) PlaceholderText.Visibility = Visibility.Collapsed;

                    // 3. Конвертируем BitmapSource в byte[] для сохранения в данные плейлиста
                    byte[] imageBytes = BitmapSourceToByteArray(cropper.ResultImage);

                    if (DataContext is Playlist playlist)
                    {
                        playlist.CoverImage = imageBytes;
                    }
                }
            }
        }
        private static byte[] BitmapSourceToByteArray(BitmapSource bitmapSource)
        {
            using var stream = new MemoryStream();
            // Используем PngBitmapEncoder для сохранения прозрачности и качества
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(stream);
            return stream.ToArray();
        }

        // Метод для кнопки "Сохранить" (обратите внимание на маленькую 'c' в 'click' в вашем XAML)
        private void EditPlaylist_Сlick(object sender, RoutedEventArgs e)
        {
            string newName = PlaylistNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                // Используем твое новое кастомное окно вместо MessageBox
                NotificationWindow.Show("Название не может быть пустым!", this);
                return;
            }

            // Проверяем, есть ли уже плейлист с таким именем (исключая текущий)
            var currentPlaylist = DataContext as Playlist;
            bool exists = MusicLibrary.Instance.Playlists.Any(p =>
                p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && p != currentPlaylist);

            if (exists)
            {
                NotificationWindow.Show("Плейлист с таким названием уже существует!", this);
                return;
            }

            DialogResult = true;
        }

        // Метод для кнопки "Отмена"
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}