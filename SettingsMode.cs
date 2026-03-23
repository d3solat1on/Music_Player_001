using System.IO;
using System.Text.Json;
namespace QAMP;

public class AppSettings
{
    public bool IsVisualizerEnabled { get; set; } = true;
    public string ColorScheme { get; set; } = "Dark"; // "Dark", "Light", "Custom"
    public string AccentColor { get; set; } = "#1db954"; // Главный цвет приложения
    public double[] EqualizerGains { get; set; } = new double[10]; // Для будущего эквалайзера
}
public class SettingsManager
{
    private static SettingsManager? _instance;
    public static SettingsManager Instance => _instance ??= new SettingsManager();

    private readonly string _path = "settings.json";
    public AppSettings Config { get; private set; }

    public SettingsManager()
    {
        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            Config = JsonSerializer.Deserialize<AppSettings>(json);
        }
        else
        {
            Config = new AppSettings();
        }
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(Config);
        File.WriteAllText(_path, json);
    }
}