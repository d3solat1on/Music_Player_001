using System.ComponentModel;
using System.IO;
using System.Text.Json;
namespace QAMP.Models;

public class AppSettings : INotifyPropertyChanged
{
    public bool CloseToTray { get; set; } = true;
    public bool IsVisualizerEnabled { get; set; } = true;
    public int VisualizerBarCount { get; set; } = 64; // Количество столбцов спектрограммы
    public string ColorScheme { get; set; } = "Dark"; // "Dark", "Light", "Custom"
    public string AccentColor { get; set; } = "#1db954"; // Главный цвет приложения
    public double[] EqualizerGains { get; set; } = new double[10]; // Значения эквалайзера
    public double[] CurrentEqualizerValues { get; set; } = new double[10];
    public string EqualizerPreset { get; set; } = "Пользовательский"; // Текущий выбранный режим
    private bool _isCompactMode = true;
    public bool UseAdaptiveGradients { get; set; } = true;
    public bool IsCompactMode 
    { 
        get => _isCompactMode;
        set { 
            _isCompactMode = value; 
            OnPropertyChanged(nameof(IsCompactMode)); 
        } 
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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