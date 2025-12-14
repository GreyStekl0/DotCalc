using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotCalc.Services;

namespace DotCalc.Models
{
    public class MemoryItem : INotifyPropertyChanged
    {
        private double _value;
        private bool _isHovered;

        /// <summary>
        /// ID элемента в базе данных.
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

        public string DisplayValue => NumberFormatter.FormatNumber(Value);

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
    }
}
