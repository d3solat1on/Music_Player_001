using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QAMP.Models;

namespace QAMP.Services
{
    public static class ThemeManager
    {
        public static void ApplyTheme(string themeName)
        {
            var app = Application.Current;
            var resources = app.Resources.MergedDictionaries;

            string themePath = $"Themes/{themeName}Theme.xaml";

            // Попытка подгрузить новую тему, прежде чем убрать старую, чтобы не было провалов в ресурсах
            try
            {
                var newTheme = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };
                resources.Add(newTheme);
            }
            catch (Exception ex)
            {
                // если тема не найдена, оставляем текущую
                System.Diagnostics.Debug.WriteLine($"ThemeManager: Не удалось загрузить тему {themeName}: {ex.Message}");
                return;
            }

            // Удалить старую тему (если есть)
            var currentTheme = resources.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme") && !d.Source.OriginalString.EndsWith($"{themeName}Theme.xaml", StringComparison.OrdinalIgnoreCase));
            if (currentTheme != null)
            {
                resources.Remove(currentTheme);
            }

            // Обновить AccentBrush
            try
            {
                var accentBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(SettingsManager.Instance.Config.AccentColor));
                app.Resources["AccentBrush"] = accentBrush;
            }
            catch
            {
                // некорректный цвет акцента не критично
            }
        }

        public static void UpdateAccentColor(string colorHex)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(colorHex) || !colorHex.StartsWith("#")) return;

                var app = Application.Current;
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
                var accentBrush = new System.Windows.Media.SolidColorBrush(color);

                accentBrush.Freeze();
                app.Resources["AccentBrush"] = accentBrush;
            }
            catch
            {
            }
        }
    }
    public class ThemeHelper
    {
        public static Color GetDominantColor(BitmapSource bitmapSource)
        {
            var colorThief = new ColorThiefDotNet.ColorThief();
            // ColorThief работает с Bitmap, поэтому конвертируем
            using var memoryStream = new System.IO.MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);
            using var bitmap = new System.Drawing.Bitmap(memoryStream);
            var quantizeColor = colorThief.GetColor(bitmap);
            return Color.FromRgb(quantizeColor.Color.R, quantizeColor.Color.G, quantizeColor.Color.B);
        }
    }
    public class DarkContextMenuRenderer : System.Windows.Forms.ToolStripProfessionalRenderer
    {
        public DarkContextMenuRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderMenuItemBackground(System.Windows.Forms.ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                System.Drawing.Color fill = System.Drawing.Color.FromArgb(60, 60, 60);
                using var brush = new System.Drawing.SolidBrush(fill);
                e.Graphics.FillRectangle(brush, new System.Drawing.Rectangle(System.Drawing.Point.Empty, e.Item.Size));
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        protected override void OnRenderToolStripBorder(System.Windows.Forms.ToolStripRenderEventArgs e) { }
    }

    public class DarkColorTable : System.Windows.Forms.ProfessionalColorTable
    {
        public override System.Drawing.Color ToolStripDropDownBackground => System.Drawing.Color.FromArgb(30, 30, 30);

        public override System.Drawing.Color MenuItemSelected => System.Drawing.Color.FromArgb(60, 60, 60);

        public override System.Drawing.Color MenuItemBorder => System.Drawing.Color.Transparent;

        public override System.Drawing.Color MenuItemSelectedGradientBegin => System.Drawing.Color.FromArgb(60, 60, 60);
        public override System.Drawing.Color MenuItemSelectedGradientEnd => System.Drawing.Color.FromArgb(60, 60, 60);

        public override System.Drawing.Color SeparatorDark => System.Drawing.Color.FromArgb(80, 80, 80);
        public override System.Drawing.Color SeparatorLight => System.Drawing.Color.Transparent;
    }
}