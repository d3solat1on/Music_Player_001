// using System.ComponentModel;
// archive
// namespace QAMP.Visualization
// {
//     public class BarItem : INotifyPropertyChanged
//     {
//         private double _value;
//         public double Value
//         {
//             get => _value;
//             set
//             {
//                 _value = value;
//                 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
//             }
//         }
//         private double _peakValue;
//         public double PeakValue
//         {
//             get => _peakValue;
//             set
//             {
//                 _peakValue = value;
//                 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PeakValue)));
//             }
//         }

//         public event PropertyChangedEventHandler PropertyChanged;
//     }
// }