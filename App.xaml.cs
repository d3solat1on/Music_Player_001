using System;
using System.Windows;
using System.Windows.Threading;

namespace MusicPlayer_by_d3solat1on
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var msg = $"Произошла ошибка: {e.Exception.Message}\n" +
                      $"Подробности: {e.Exception.StackTrace}";
            if (e.Exception.InnerException != null)
            {
                msg += "\n\nВнутреннее исключение: " + e.Exception.InnerException.Message +
                       "\n" + e.Exception.InnerException.StackTrace;
            }
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Предотвращаем закрытие программы
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show($"Критическая ошибка: {ex?.Message}", 
                           "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}