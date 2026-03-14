using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusicPlayer_by_d3solat1on.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Безопасное приведение типа
            if (value is not byte[] bytes || bytes.Length == 0)
                return null;

            try
            {
                var image = new BitmapImage();
                using (var stream = new MemoryStream(bytes))
                {
                    image.BeginInit();
                    // Важно для экономии памяти в списках:
                    image.DecodePixelWidth = 350; // Можно ограничить размер, так как это иконка в списке
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                image.Freeze(); // Делает объект доступным для других потоков и ускоряет рендеринг
                return image;
            }
            catch
            {
                return null; // Если массив байтов не является валидным изображением
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}