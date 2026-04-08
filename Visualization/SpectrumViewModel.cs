// using System.Collections.ObjectModel;
// archive
// namespace QAMP.Visualization
// {
//     public class SpectrumViewModel
//     {
//         public ObservableCollection<BarItem> Bars { get; }
//         private int _barCount = 48;
//         private readonly Lock _barsLock = new();

        
//         private readonly Queue<float[]> _fftHistory = new();
//         private const int HistorySize = 3; 

//         public int BarCount
//         {
//             get => _barCount;
//             set
//             {
//                 if (_barCount == value) return;
//                 _barCount = value;
//                 ReinitializeBars();
//             }
//         }

//         public SpectrumViewModel()
//         {
//             Bars = [];
//             ReinitializeBars();
//         }

//         private void ReinitializeBars()
//         {
//             lock (_barsLock)
//             {
//                 Bars.Clear();
//                 for (int i = 0; i < _barCount; i++)
//                     Bars.Add(new BarItem { Value = 1 });
//             }
//         }

        
//         private static float[] ApplyMedianFilter(float[] data, int windowSize = 4)
//         {
//             var result = new float[data.Length];
//             int halfWindow = windowSize / 2;

//             for (int i = 0; i < data.Length; i++)
//             {
//                 var window = new List<float>();
//                 for (int j = -halfWindow; j <= halfWindow; j++)
//                 {
//                     int idx = Math.Clamp(i + j, 0, data.Length - 1);
//                     window.Add(data[idx]);
//                 }
//                 window.Sort();
//                 result[i] = window[window.Count / 2];
//             }
//             return result;
//         }

        
//         private float[] ApplyTemporalMedianFilter(float[] currentData)
//         {
            
//             _fftHistory.Enqueue((float[])currentData.Clone());

            
//             while (_fftHistory.Count > HistorySize)
//                 _fftHistory.Dequeue();

            
//             if (_fftHistory.Count < HistorySize)
//                 return currentData;

            
//             var result = new float[currentData.Length];
//             var historyArray = _fftHistory.ToArray();

//             for (int i = 0; i < currentData.Length; i++)
//             {
//                 var temporalWindow = new List<float>();
//                 for (int h = 0; h < historyArray.Length; h++)
//                 {
//                     temporalWindow.Add(historyArray[h][i]);
//                 }
//                 temporalWindow.Sort();
//                 result[i] = temporalWindow[temporalWindow.Count / 2];
//             }

//             return result;
//         }

//         public void Update(float[] fftData)
//         {
//             int halfLength = fftData.Length;

            
//             float[] filteredData = ApplyMedianFilter(fftData, 4);

            
//             filteredData = ApplyTemporalMedianFilter(filteredData);
            

//             double[] newValues = new double[_barCount];

//             lock (_barsLock)
//             {
//                 double step = 1.0 / _barCount;

                
//                 for (int i = 0; i < _barCount; i++)
//                 {
//                     double logPercent = Math.Pow(i * step, 1.7);
//                     double nextLogPercent = Math.Pow((i + 1) * step, 1.7);

//                     int startIndex = (int)(logPercent * (halfLength - 1));
//                     int endIndex = (int)(nextLogPercent * (halfLength - 1));
//                     if (endIndex <= startIndex) endIndex = startIndex + 1;

                    
//                     float currentMaxInBand = 0;
//                     for (int j = startIndex; j < endIndex && j < halfLength; j++)
//                     {
//                         if (filteredData[j] > currentMaxInBand) currentMaxInBand = filteredData[j];
//                     }

                    
//                     newValues[i] = Math.Sqrt(currentMaxInBand) * 300.0;
//                 }

                
//                 double[] smoothedValues = new double[_barCount];
//                 for (int i = 0; i < _barCount; i++)
//                 {
//                     double sum = newValues[i];
//                     int count = 1;

//                     if (i > 0) { sum += newValues[i - 1]; count++; }
//                     if (i < _barCount - 1) { sum += newValues[i + 1]; count++; }
//                     if (i > 1) { sum += newValues[i - 2] * 0.5; count++; }
//                     if (i < _barCount - 2) { sum += newValues[i + 2] * 0.5; count++; }

//                     smoothedValues[i] = sum / count;
//                 }

//                 for (int i = 1; i < _barCount - 1; i++)
//                 {
                    
//                     newValues[i] = (newValues[i - 1] * 0.25) + (newValues[i] * 0.65) + (newValues[i + 1] * 0.25);
//                 }
                
//                 for (int i = 0; i < _barCount && i < Bars.Count; i++)
//                 {
//                     double newValueRaw = smoothedValues[i];
//                     double newValue = Math.Min(1.0, Math.Max(0.03, newValueRaw / 35.0));

//                     double oldValue = Bars[i].Value;
//                     double finalValue;

//                     if (newValue > oldValue)
//                         finalValue = oldValue + (newValue - oldValue) * 0.85; 
//                     else
//                         finalValue = oldValue * 0.78; 

//                     Bars[i].Value = finalValue;
//                     double peakDecay = 0.85;
//                     double newPeak = Bars[i].PeakValue * peakDecay;
//                     if (finalValue > newPeak)
//                         newPeak = finalValue;
//                     Bars[i].PeakValue = newPeak;
//                 }
//             }
//         }
//     }
// }