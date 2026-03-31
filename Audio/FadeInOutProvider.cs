using NAudio.Wave;
namespace QAMP.Audio;

public class FadeInOutProvider(ISampleProvider source) : ISampleProvider
{
    private readonly ISampleProvider _source = source;
    private int _fadeSampleCount;
    private int _fadeSamplesRemaining;
    private bool _fadingOut;
    private float _fadeStep;
    private float _currentGain = 1.0f;
    private readonly Lock _lockObject = new();

    public WaveFormat WaveFormat => _source.WaveFormat;

    // Метод для плавного затухания (Fade Out)
    public void ResetGain()
    {
        lock (_lockObject)
        {
            _currentGain = 0f;
            _fadeSamplesRemaining = 0;
        }
    }
    public void BeginFadeOut(int fadeMilliseconds)
    {
        lock (_lockObject)
        {
            _fadeSampleCount = WaveFormat.SampleRate * WaveFormat.Channels * fadeMilliseconds / 1000;
            _fadeSamplesRemaining = _fadeSampleCount;
            _fadingOut = true;
            _fadeStep = _currentGain / _fadeSampleCount; // Шаг зависит от текущей громкости
        }
    }

    // Метод для плавного появления (Fade In)
    public void BeginFadeIn(int fadeMilliseconds)
    {
        lock (_lockObject)
        {
            _fadeSampleCount = WaveFormat.SampleRate * WaveFormat.Channels * fadeMilliseconds / 1000;
            _fadeSamplesRemaining = _fadeSampleCount;
            _fadingOut = false;
            _fadeStep = (1.0f - _currentGain) / _fadeSampleCount;
        }
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int sourceRead = _source.Read(buffer, offset, count);

        lock (_lockObject)
        {
            if (_fadeSamplesRemaining > 0)
            {
                for (int i = 0; i < sourceRead; i++)
                {
                    buffer[offset + i] *= _currentGain;

                    if (_fadingOut)
                    {
                        _currentGain -= _fadeStep;
                        if (_currentGain < 0) _currentGain = 0;
                    }
                    else
                    {
                        _currentGain += _fadeStep;
                        if (_currentGain > 1) _currentGain = 1;
                    }

                    _fadeSamplesRemaining--;
                    if (_fadeSamplesRemaining <= 0) break;
                }
            }
            else if (_fadingOut)
            {
                // Если затухание закончилось, заполняем остаток буфера тишиной
                for (int i = 0; i < sourceRead; i++)
                {
                    buffer[offset + i] = 0;
                }
            }
        }

        return sourceRead;
    }
}