namespace QAMP.Visualization;

public class SpectrumSettings
{
    public int BarCount { get; set; } = 48;
    public int PointCount { get; set; } = 256;
    public double FreqPower { get; set; } = 1.2;
    public double AmplitudeGain { get; set; } = 55.0;   
    public double AmplitudePower { get; set; } = 0.5;   
    public bool AutoNormalize { get; set; } = false;    
    public double MaxGainLimit { get; set; } = 50.0;
    public double MinBarValue { get; set; } = 0.00;
    public double MaxBarValue { get; set; } = 0.95;
    public double AttackSpeed { get; set; } = 0.6;
    public double ReleaseSpeed { get; set; } = 0.8;
    public double BarWidth { get; set; } = 0.8;
    public double BackgroundAlpha { get; set; } = 1.0;
    
    public void ApplyPreset(string presetName)
    {
        switch (presetName.ToLower())
        {
            case "default":
                FreqPower = 1.2;
                AmplitudeGain = 15.0;
                AmplitudePower = 0.5;
                AttackSpeed = 0.4;
                ReleaseSpeed = 0.92;
                AutoNormalize = false;
                break;
                
            case "bass_heavy":
                FreqPower = 2.0;
                AmplitudeGain = 20.0;
                AmplitudePower = 0.4;
                AttackSpeed = 0.6;
                ReleaseSpeed = 0.95;
                break;
                
            case "balanced":
                FreqPower = 1.5;
                AmplitudeGain = 15.0;
                AmplitudePower = 0.5;
                AttackSpeed = 0.5;
                ReleaseSpeed = 0.9;
                break;
                
            case "sensitive":
                FreqPower = 1.0;
                AmplitudeGain = 25.0;
                AmplitudePower = 0.4;
                AttackSpeed = 0.7;
                ReleaseSpeed = 0.85;
                break;
        }
    }
}