using System.Collections.ObjectModel;

namespace QAMP.Visualization
{
    public class SpectrumViewModel
    {
        public ObservableCollection<BarItem> Bars { get; }
        private readonly int BarCount = 64;

        public SpectrumViewModel()
        {
            Bars = [];
            for (int i = 0; i < BarCount; i++)
                Bars.Add(new BarItem { Value = 1 }); 
        }

        public void Update(float[] fftData)
        {
            int halfLength = fftData.Length; // В SampleAggregator мы уже передаем половину (1024)

            for (int i = 0; i < BarCount; i++)
            {
                // Твоя логика распределения частот из Form1.cs
                double percent = (double)i / BarCount;
                double logPercent = Math.Pow(percent, 1.5);

                int startIndex = (int)(logPercent * (halfLength - 10));
                int endIndex = (int)(Math.Pow((double)(i + 1) / BarCount, 2) * (halfLength - 10));
                endIndex = Math.Max(startIndex + 1, endIndex);

                float maxInBand = 0;
                for (int j = startIndex; j < endIndex && j < halfLength; j++)
                {
                    if (fftData[j] > maxInBand) maxInBand = fftData[j];
                }
                double newValue = Math.Pow(maxInBand * 1000, 0.5) * 0.5;
                newValue *= 225;
                if (newValue > 100) newValue = 100;
                if (newValue < 2) newValue = 2; 

                if (newValue > Bars[i].Value)
                    Bars[i].Value = newValue;
                else
                    Bars[i].Value *= 0.35; // Быстрое падение для отзывчивого спектра
            }
        }
    }
}