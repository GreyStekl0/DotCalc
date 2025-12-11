using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotCalc.Models
{
    public class MemoryItem : INotifyPropertyChanged
    {
        private double _value;
        private bool _isHovered;

        /// <summary>
        /// ID записи в базе данных
        /// </summary>
        public int DatabaseId { get; set; }

        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayValue));
                }
            }
        }

        public string DisplayValue => FormatNumber(Value);

        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered != value)
                {
                    _isHovered = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
            {
                return value.ToString("0");
            }
            return value.ToString("G15").TrimEnd('0').TrimEnd(
                System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
        }
    }
}
