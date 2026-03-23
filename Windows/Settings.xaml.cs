using System.Windows;
using System.Windows.Controls;
using QAMP;
using QAMP.Services;
using QAMP.ViewModels;

namespace QAMP.Windows
{
    public partial class Settings : Window
    {
        private bool isInitializing;
        private string originalColorScheme;
        private string originalAccentColor;
        private  PlayerService _player;

        public Settings(PlayerService player)
        {
            InitializeComponent();
             _player = player;
            LoadEqualizerData();
        }

        private void LoadEqualizerData()
        {
            var bands = new List<EqBandViewModel>();
            float[] freqs = [31, 62, 125, 250, 500, 1000, 2000, 4000, 8000, 16000];

            for (int i = 0; i < freqs.Length; i++)
            {
                string freqLabel = freqs[i] < 1000 ? $"{freqs[i]}" : $"{freqs[i] / 1000}k";
                bands.Add(new EqBandViewModel
                {
                    Index = i,
                    Frequency = freqLabel,
                    // Считываем значение из плеера, а не ставим 0!
                    Gain = _player.EqGains[i]
                });
            }
            EqItemsControl.ItemsSource = bands;
        }

        private void EqSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider && slider.DataContext is EqBandViewModel band)
            {
                float newValue = (float)e.NewValue;
                _player.CurrentEqualizer?.SetGain(band.Index, newValue);
                // Сохраняем в память плеера, чтобы при переоткрытии окна данные подтянулись
                _player.EqGains[band.Index] = newValue;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isInitializing = true;

            var config = SettingsManager.Instance.Config;
            originalColorScheme = config.ColorScheme;
            originalAccentColor = config.AccentColor;

            // Установить выбранную тему (без повторного применения в событие)
            switch (config.ColorScheme)
            {
                case "Dark":
                    DarkThemeRadio.IsChecked = true;
                    break;
                case "Light":
                    LightThemeRadio.IsChecked = true;
                    break;
                case "Custom":
                    CustomThemeRadio.IsChecked = true;
                    break;
            }

            // Установить акцентный цвет
            AccentColorTextBox.Text = config.AccentColor;
            UpdateColorPreview();
           
            isInitializing = false;
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if (sender is not RadioButton radio) return;

            string theme = radio.Content.ToString() switch
            {
                "Темная" => "Dark",
                "Светлая" => "Light",
                "Пользовательская" => "Custom",
                _ => "Dark"
            };

            if (SettingsManager.Instance.Config.ColorScheme == theme)
                return;

            SettingsManager.Instance.Config.ColorScheme = theme;

            // Не применяем несуществующую тему
            if (theme == "Custom")
            {
                // Просто оставляем текущую тему и применяем оттенок
                ThemeManager.UpdateAccentColor(SettingsManager.Instance.Config.AccentColor);
                return;
            }

            ThemeManager.ApplyTheme(theme);
        }

        private void AccentColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = SettingsManager.Instance.Config;
            config.AccentColor = AccentColorTextBox.Text;
            ThemeManager.UpdateAccentColor(config.AccentColor);
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            try
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(AccentColorTextBox.Text);
                ColorPreview.Text = $"Предварительный просмотр: RGB({color.R}, {color.G}, {color.B})";
                ColorPreview.Foreground = new System.Windows.Media.SolidColorBrush(color);
            }
            catch
            {
                ColorPreview.Text = "Неверный формат цвета";
                ColorPreview.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.Save();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Восстановить оригинальные настройки
            SettingsManager.Instance.Config.ColorScheme = originalColorScheme;
            SettingsManager.Instance.Config.AccentColor = originalAccentColor;
            ThemeManager.ApplyTheme(originalColorScheme);
            ThemeManager.UpdateAccentColor(originalAccentColor);
            DialogResult = false;
            Close();
        }
    }
}