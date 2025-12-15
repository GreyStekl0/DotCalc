using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotCalc.Services;

namespace DotCalc.Models
{
    /// <summary>
    /// Элемент "памяти" калькулятора (MR/M+/M-/MS): хранится в списке и синхронизируется с БД.
    /// </summary>
    /// <remarks>
    /// Реализует <see cref="INotifyPropertyChanged"/> для обновления привязок в UI (MAUI).
    /// </remarks>
    public class MemoryItem : INotifyPropertyChanged
    {
        private double _value;
        private bool _isHovered;

        /// <summary>
        /// ID элемента в базе данных (таблица <c>MemoryItemEntity</c>).
        /// </summary>
        public int DatabaseId { get; set; }

        /// <summary>
        /// Числовое значение, сохраненное в памяти.
        /// </summary>
        public double Value
        {
            get => _value;
            set
            {
                if (!(Math.Abs(_value - value) > double.Epsilon)) return;
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayValue));
            }
        }

        /// <summary>
        /// Строка для отображения <see cref="Value"/> с учетом форматирования (культура, экспонента, нули).
        /// </summary>
        public string DisplayValue => NumberFormatter.FormatNumber(Value);

        /// <summary>
        /// Флаг подсветки элемента в UI (например, при наведении курсора).
        /// </summary>
        public bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value) return;
                _isHovered = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Событие уведомления об изменении свойства.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Уведомляет подписчиков, что свойство изменилось.
        /// </summary>
        /// <param name="propertyName">Имя свойства (автоматически подставляется компилятором).</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
