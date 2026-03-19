using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using QAMP.Models;

namespace QAMP.Dialogs
{
    public partial class CreatePlaylistDialog : Window
    {
        public string PlaylistName { get; private set; }
        public string PlaylistDescription { get; private set; }
        public byte[] PlaylistCoverImage { get; private set; }

        public CreatePlaylistDialog(Playlist existingPlaylist = null)
        {
            InitializeComponent();
        }


        private void SelectCoverButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var cropper = new ImageCropperDialog(openFileDialog.FileName)
                {
                    Owner = this
                };

                if (cropper.ShowDialog() == true)
                {
                    CoverImage.Source = cropper.ResultImage;
                    PlaceholderText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PlaylistNameTextBox.Text))
            {
                NotificationWindow.Show("Введите название плейлиста", this);
                return;
            }

            PlaylistName = PlaylistNameTextBox.Text.Trim();
            PlaylistDescription = PlaylistDescriptionTextBox.Text?.Trim() ?? "";

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}