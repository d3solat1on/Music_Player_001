using System;
using System.Windows;
using System.Windows.Threading;
using QAMP.Services;

namespace QAMP
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // DatabaseService.EnsureDatabaseCreated();

            // Применить тему при запуске
            ThemeManager.ApplyTheme(SettingsManager.Instance.Config.ColorScheme);
        }
    }
}