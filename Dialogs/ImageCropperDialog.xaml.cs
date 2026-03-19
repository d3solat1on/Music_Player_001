using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QAMP.Dialogs
{
    public partial class ImageCropperDialog : Window
    {
        private Point _startPoint;
        private double _originX, _originY;
        public BitmapSource? ResultImage { get; private set; }

        public ImageCropperDialog(string imagePath)
        {
            InitializeComponent();
            var bitmap = new BitmapImage(new Uri(imagePath));
            SourceImage.Source = bitmap;

            // Ждем загрузки, чтобы узнать реальные размеры
            SourceImage.Loaded += (s, e) =>
            {
                // Вычисляем масштаб, чтобы картинка заполнила квадрат по меньшей стороне
                double scale = Math.Max(300 / bitmap.Width, 300 / bitmap.Height);
                ImageScale.ScaleX = scale;
                ImageScale.ScaleY = scale;

                // Центрируем
                Canvas.SetLeft(SourceImage, (300 - bitmap.Width * scale) / 2);
                Canvas.SetTop(SourceImage, (300 - bitmap.Height * scale) / 2);

                // Обновляем слайдер, чтобы его значение соответствовало начальному масштабу
                ZoomSlider.Value = scale;
            };
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(ContainerCanvas);
            _originX = Canvas.GetLeft(SourceImage);
            _originY = Canvas.GetTop(SourceImage);

            SourceImage.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!SourceImage.IsMouseCaptured) return;

            Vector diff = e.GetPosition(ContainerCanvas) - _startPoint;

            Canvas.SetLeft(SourceImage, _originX + diff.X);
            Canvas.SetTop(SourceImage, _originY + diff.Y);
        }
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Проверка на null нужна, так как ValueChanged срабатывает при инициализации до того, как ImageScale создан
            if (ImageScale != null)
            {
                ImageScale.ScaleX = e.NewValue;
                ImageScale.ScaleY = e.NewValue;
            }
        }
        private void ContainerCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Если крутим вверх — приближаем, вниз — отдаляем
            if (e.Delta > 0)
                ZoomSlider.Value += 0.1;
            else
                ZoomSlider.Value -= 0.1;
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => SourceImage.ReleaseMouseCapture();

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            // Размер области кропа (соответствует Border в XAML)
            RenderTargetBitmap bmp = new(300, 300, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(ContainerCanvas);
            ResultImage = bmp;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}