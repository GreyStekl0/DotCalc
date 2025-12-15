using System.Collections.ObjectModel;
using System.Globalization;
using DotCalc.Models;

namespace DotCalc.Services
{
    /// <summary>
    /// Движок калькулятора: хранит состояние ввода и выполняет операции.
    /// </summary>
    /// <remarks>
    /// Класс не зависит от UI: страница лишь вызывает методы и читает <see cref="DisplayText"/>/<see cref="ExpressionText"/>.
    /// </remarks>
    public sealed class CalculatorEngine
    {
        /// <summary>
        /// Текст, который показываем в случае ошибки (деление на ноль и т.п.).
        /// </summary>
        public const string ErrorText = "Ошибка";

        // Текущее введенное число (правый операнд), которое сейчас показано на дисплее.
        private double _currentValue;

        // Сохраненное число (левый операнд) после выбора оператора.
        private double _storedValue;

        // Выбранный оператор бинарной операции ("+", "−", "×", "÷"). Пустая строка = операции нет.
        private string _currentOperator = string.Empty;

        // Флаг "новый ввод": следующая цифра должна заменить дисплей, а не дописаться к нему.
        private bool _isNewEntry = true;

        // Пара (оператор + правый операнд) для повторного нажатия "=".
        private string _lastOperator = string.Empty;
        private double _lastOperand;

        // Признак того, что последняя команда была "=" (нужно поддержать повтор "= = =").
        private bool _justCalculated;

        /// <summary>
        /// История вычислений (самый новый элемент хранится первым).
        /// </summary>
        public ObservableCollection<HistoryItem> History { get; } = [];

        /// <summary>
        /// Текст, который отображается на основном дисплее.
        /// </summary>
        public string DisplayText { get; private set; } = "0";

        /// <summary>
        /// Текст выражения над основным дисплеем (например: <c>2 + 3 =</c>).
        /// </summary>
        public string ExpressionText { get; private set; } = string.Empty;

        /// <summary>
        /// Принудительно устанавливает текст дисплея (обычно при выборе элемента истории/памяти).
        /// </summary>
        /// <param name="value">Строка, которую нужно показать на дисплее.</param>
        /// <param name="isNewEntry">Если <c>true</c>, следующая цифра начнет новый ввод.</param>
        public void SetDisplayText(string value, bool isNewEntry = true)
        {
            DisplayText = value ?? throw new ArgumentNullException(nameof(value));
            _isNewEntry = isNewEntry;
        }

        /// <summary>
        /// Очищает журнал вычислений.
        /// </summary>
        public void ClearHistory()
        {
            History.Clear();
        }

        /// <summary>
        /// Загружает выбранный элемент истории на дисплей и готовит движок к новому вводу.
        /// </summary>
        /// <param name="item">Элемент истории.</param>
        public void SelectHistoryItem(HistoryItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            ExpressionText = item.Expression;
            DisplayText = item.Result;

            if (double.TryParse(item.Result, out double result))
            {
                _storedValue = result;
            }

            _isNewEntry = true;
            _currentOperator = string.Empty;
            _justCalculated = false;
        }

        /// <summary>
        /// Вводит одну цифру (или знак минус, если UI так решит).
        /// </summary>
        /// <param name="digit">Символ цифры.</param>
        public void Digit(string digit)
        {
            ArgumentNullException.ThrowIfNull(digit);

            if (_isNewEntry)
            {
                DisplayText = digit;
                _isNewEntry = false;
                return;
            }

            if (DisplayText == "0" && digit != "0")
            {
                DisplayText = digit;
            }
            else if (DisplayText != "0")
            {
                DisplayText += digit;
            }
        }

        /// <summary>
        /// Добавляет десятичный разделитель (в зависимости от текущей культуры).
        /// </summary>
        public void Decimal()
        {
            string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (_isNewEntry)
            {
                DisplayText = "0" + separator;
                _isNewEntry = false;
            }
            else if (!DisplayText.Contains(separator, StringComparison.Ordinal))
            {
                DisplayText += separator;
            }
        }

        /// <summary>
        /// Выбирает оператор бинарной операции и сохраняет левый операнд.
        /// </summary>
        /// <param name="op">Оператор (например: <c>+</c>, <c>−</c>, <c>×</c>, <c>÷</c>).</param>
        public void Operator(string op)
        {
            ArgumentNullException.ThrowIfNull(op);

            // Если пользователь цепляет операции без "=", сначала завершаем предыдущую.
            if (!_isNewEntry && !string.IsNullOrEmpty(_currentOperator))
            {
                PerformCalculation();
            }

            _currentValue = double.TryParse(DisplayText, out double value) ? value : 0;
            _storedValue = _currentValue;
            _currentOperator = op;
            ExpressionText = NumberFormatter.FormatNumber(_storedValue) + " " + op;
            _isNewEntry = true;
            _justCalculated = false;
        }

        /// <summary>
        /// Выполняет вычисление. При повторном нажатии "=" повторяет последнюю операцию.
        /// </summary>
        public void Equals()
        {
            if (_justCalculated && !string.IsNullOrEmpty(_lastOperator))
            {
                _storedValue = double.TryParse(DisplayText, out double value) ? value : 0;

                ExpressionText = NumberFormatter.FormatNumber(_storedValue) + " " + _lastOperator + " " +
                                 NumberFormatter.FormatNumber(_lastOperand) + " =";

                double result = CalculateResult(_storedValue, _lastOperator, _lastOperand);

                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    DisplayText = ErrorText;
                    _isNewEntry = true;
                    _justCalculated = false;
                    return;
                }

                string resultText = NumberFormatter.FormatNumber(result);
                DisplayText = resultText;
                AddToHistory(ExpressionText, resultText);
                _isNewEntry = true;
            }
            else if (!string.IsNullOrEmpty(_currentOperator))
            {
                _currentValue = double.TryParse(DisplayText, out double value) ? value : 0;

                _lastOperator = _currentOperator;
                _lastOperand = _currentValue;

                ExpressionText = NumberFormatter.FormatNumber(_storedValue) + " " + _currentOperator + " " +
                                 NumberFormatter.FormatNumber(_currentValue) + " =";

                double result = CalculateResult(_storedValue, _currentOperator, _currentValue);

                // Отлавливаем ошибки вычислений, чтобы не показывать NaN/Infinity пользователю.
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    DisplayText = ErrorText;
                    _isNewEntry = true;
                    _currentOperator = string.Empty;
                    _justCalculated = false;
                    return;
                }

                _storedValue = result;
                string resultText = NumberFormatter.FormatNumber(result);
                DisplayText = resultText;
                AddToHistory(ExpressionText, resultText);

                _currentOperator = string.Empty;
                _isNewEntry = true;
                _justCalculated = true;
            }
        }

        /// <summary>
        /// Сбрасывает текущее выражение и дисплей в исходное состояние.
        /// </summary>
        public void Clear()
        {
            _currentValue = 0;
            _storedValue = 0;
            _currentOperator = string.Empty;
            _isNewEntry = true;
            _lastOperator = string.Empty;
            _lastOperand = 0;
            _justCalculated = false;
            DisplayText = "0";
            ExpressionText = string.Empty;
        }

        /// <summary>
        /// Очищает только текущий ввод (дисплей), не трогая выражение и историю.
        /// </summary>
        public void ClearEntry()
        {
            DisplayText = "0";
            _isNewEntry = true;
        }

        /// <summary>
        /// Удаляет последний символ на дисплее.
        /// </summary>
        public void Backspace()
        {
            if (_isNewEntry || DisplayText.Length == 0)
            {
                return;
            }

            string current = DisplayText;
            if (current.Length == 1 || (current.Length == 2 && current.StartsWith("-", StringComparison.Ordinal)))
            {
                DisplayText = "0";
                _isNewEntry = true;
            }
            else
            {
                DisplayText = current[..^1];
            }
        }

        /// <summary>
        /// Меняет знак текущего числа.
        /// </summary>
        public void Negate()
        {
            if (double.TryParse(DisplayText, out double value) && value != 0)
            {
                value = -value;
                DisplayText = NumberFormatter.FormatNumber(value);
            }
        }

        /// <summary>
        /// Применяет проценты. Если выбран оператор, вычисляет процент от сохраненного значения.
        /// </summary>
        public void Percent()
        {
            if (double.TryParse(DisplayText, out double value))
            {
                if (!string.IsNullOrEmpty(_currentOperator))
                {
                    value = _storedValue * (value / 100);
                }
                else
                {
                    value = value / 100;
                }

                DisplayText = NumberFormatter.FormatNumber(value);
                _isNewEntry = true;
            }
        }

        /// <summary>
        /// Возводит текущее значение в квадрат.
        /// </summary>
        public void Square()
        {
            if (double.TryParse(DisplayText, out double value))
            {
                string originalValue = NumberFormatter.FormatNumber(value);
                double result = value * value;
                ExpressionText = $"sqr({originalValue})";
                DisplayText = NumberFormatter.FormatNumber(result);
                _isNewEntry = true;
            }
        }

        /// <summary>
        /// Извлекает квадратный корень из текущего значения.
        /// </summary>
        public void SquareRoot()
        {
            if (double.TryParse(DisplayText, out double value))
            {
                if (value < 0)
                {
                    DisplayText = ErrorText;
                    _isNewEntry = true;
                    return;
                }

                string originalValue = NumberFormatter.FormatNumber(value);
                double result = Math.Sqrt(value);
                ExpressionText = $"√({originalValue})";
                DisplayText = NumberFormatter.FormatNumber(result);
                _isNewEntry = true;
            }
        }

        /// <summary>
        /// Вычисляет обратное значение (<c>1/x</c>) для текущего числа.
        /// </summary>
        public void Inverse()
        {
            if (double.TryParse(DisplayText, out double value))
            {
                if (value == 0)
                {
                    DisplayText = ErrorText;
                    _isNewEntry = true;
                    return;
                }

                string originalValue = NumberFormatter.FormatNumber(value);
                double result = 1 / value;
                ExpressionText = $"1/({originalValue})";
                DisplayText = NumberFormatter.FormatNumber(result);
                _isNewEntry = true;
            }
        }

        // Добавляем новую строку в журнал (в начало списка, чтобы "последнее сверху").
        private void AddToHistory(string expression, string result)
        {
            History.Insert(0, new HistoryItem
            {
                Expression = expression,
                Result = result
            });
        }

        // Единая точка вычисления бинарных операций.
        private static double CalculateResult(double left, string op, double right)
        {
            return op switch
            {
                "+" => left + right,
                "−" => left - right,
                "×" => left * right,
                "÷" => right != 0 ? left / right : double.NaN,
                _ => right
            };
        }

        // Служебный шаг для "цепочки" операторов без нажатия "=".
        private void PerformCalculation()
        {
            _currentValue = double.TryParse(DisplayText, out double value) ? value : 0;

            double result = CalculateResult(_storedValue, _currentOperator, _currentValue);

            // Отлавливаем ошибки вычислений, чтобы не оставлять движок в некорректном состоянии.
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                DisplayText = ErrorText;
                _isNewEntry = true;
                _currentOperator = string.Empty;
                return;
            }

            _storedValue = result;
            DisplayText = NumberFormatter.FormatNumber(result);
            _isNewEntry = true;
        }
    }
}
