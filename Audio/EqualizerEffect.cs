using NAudio.Dsp;
using NAudio.Wave;

namespace QAMP.Audio
{
    public class EqualizerFilter : ISampleProvider
    {
        private readonly ISampleProvider _sourceProvider;
        private readonly BiQuadFilter[] _filters;
        private readonly float[] _frequencies;
        private readonly int _sampleRate;

        public WaveFormat WaveFormat => _sourceProvider.WaveFormat;

        public EqualizerFilter(ISampleProvider sourceProvider, float[] frequencies)
        {
            _sourceProvider = sourceProvider;
            _frequencies = frequencies;
            _sampleRate = sourceProvider.WaveFormat.SampleRate;
            _filters = new BiQuadFilter[frequencies.Length];

            for (int i = 0; i < frequencies.Length; i++)
            {
                // Q-factor 0.8f обычно хорошо подходит для 10-полосного эквалайзера
                _filters[i] = BiQuadFilter.PeakingEQ(_sampleRate, _frequencies[i], 0.8f, 0);
            }
        }

        public void SetGain(int index, float gain)
        {
            if (index < 0 || index >= _filters.Length) return;

            // Обновляем существующий фильтр новыми коэффициентами
            _filters[index].SetPeakingEq(_sampleRate, _frequencies[index], 0.8f, gain);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            // Читаем данные из источника (AudioFileReader)
            int samplesRead = _sourceProvider.Read(buffer, offset, count);

            // ОПТИМИЗАЦИЯ: применяем фильтры для каждого сэмпла
            // Это более эффективно, чем проходить все сэмплы для каждого фильтра
            for (int n = 0; n < samplesRead; n++)
            {
                float sample = buffer[offset + n];
                // Применяем все фильтры к одному сэмплу последовательно
                for (int i = 0; i < _filters.Length; i++)
                {
                    sample = _filters[i].Transform(sample);
                }
                buffer[offset + n] = sample;
            }

            return samplesRead;
        }
    }
}