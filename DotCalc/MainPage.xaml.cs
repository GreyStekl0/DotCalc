namespace DotCalc
{
    public partial class MainPage : ContentPage
    {
        private double _currentValue = 0;
        private double _storedValue = 0;
        private string _currentOperator = "";
        private bool _isNewEntry = true;
        private double _memory = 0;
        private string _expression = "";

        public MainPage()
        {
            InitializeComponent();
        }

        private void UpdateDisplay(string value)
        {
            DisplayLabel.Text = value;
        }

        private void UpdateExpression(string expression)
        {
            ExpressionLabel.Text = expression;
        }

        private void OnDigit(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string digit = button.Text;
                
                if (_isNewEntry)
                {
                    DisplayLabel.Text = digit;
                    _isNewEntry = false;
                }
                else
                {
                    if (DisplayLabel.Text == "0" && digit != "0")
                    {
                        DisplayLabel.Text = digit;
                    }
                    else if (DisplayLabel.Text != "0")
                    {
                        DisplayLabel.Text += digit;
                    }
                }
            }
        }

        private void OnDecimal(object? sender, EventArgs e)
        {
            string separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            
            if (_isNewEntry)
            {
                DisplayLabel.Text = "0" + separator;
                _isNewEntry = false;
            }
            else if (!DisplayLabel.Text.Contains(separator))
            {
                DisplayLabel.Text += separator;
            }
        }

        private void OnOperator(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                string op = button.Text;
                
                if (!_isNewEntry && !string.IsNullOrEmpty(_currentOperator))
                {
                    PerformCalculation();
                }
                
                _currentValue = double.TryParse(DisplayLabel.Text, out double val) ? val : 0;
                _storedValue = _currentValue;
                _currentOperator = op;
                _expression = FormatNumber(_storedValue) + " " + op;
                UpdateExpression(_expression);
                _isNewEntry = true;
            }
        }

        private void OnEquals(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentOperator))
            {
                _currentValue = double.TryParse(DisplayLabel.Text, out double val) ? val : 0;
                _expression = FormatNumber(_storedValue) + " " + _currentOperator + " " + FormatNumber(_currentValue) + " =";
                UpdateExpression(_expression);
                
                PerformCalculation();
                _currentOperator = "";
            }
        }

        private void PerformCalculation()
        {
            _currentValue = double.TryParse(DisplayLabel.Text, out double val) ? val : 0;
            
            double result = _currentOperator switch
            {
                "+" => _storedValue + _currentValue,
                "−" => _storedValue - _currentValue,
                "×" => _storedValue * _currentValue,
                "÷" => _currentValue != 0 ? _storedValue / _currentValue : double.NaN,
                _ => _currentValue
            };
            
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                UpdateDisplay("Ошибка");
                _isNewEntry = true;
                _currentOperator = "";
                return;
            }
            
            _storedValue = result;
            UpdateDisplay(FormatNumber(result));
            _isNewEntry = true;
        }

        private void OnClear(object? sender, EventArgs e)
        {
            _currentValue = 0;
            _storedValue = 0;
            _currentOperator = "";
            _isNewEntry = true;
            _expression = "";
            UpdateDisplay("0");
            UpdateExpression("");
        }

        private void OnClearEntry(object? sender, EventArgs e)
        {
            UpdateDisplay("0");
            _isNewEntry = true;
        }

        private void OnBackspace(object? sender, EventArgs e)
        {
            if (!_isNewEntry && DisplayLabel.Text.Length > 0)
            {
                string current = DisplayLabel.Text;
                if (current.Length == 1 || (current.Length == 2 && current.StartsWith("-")))
                {
                    UpdateDisplay("0");
                    _isNewEntry = true;
                }
                else
                {
                    UpdateDisplay(current[..^1]);
                }
            }
        }

        private void OnNegate(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value) && value != 0)
            {
                value = -value;
                UpdateDisplay(FormatNumber(value));
            }
        }

        private void OnPercent(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                if (!string.IsNullOrEmpty(_currentOperator))
                {
                    value = _storedValue * (value / 100);
                }
                else
                {
                    value = value / 100;
                }
                UpdateDisplay(FormatNumber(value));
                _isNewEntry = true;
            }
        }

        private void OnSquare(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                string originalValue = FormatNumber(value);
                double result = value * value;
                _expression = $"sqr({originalValue})";
                UpdateExpression(_expression);
                UpdateDisplay(FormatNumber(result));
                _isNewEntry = true;
            }
        }

        private void OnSquareRoot(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                if (value < 0)
                {
                    UpdateDisplay("Ошибка");
                    _isNewEntry = true;
                    return;
                }
                
                string originalValue = FormatNumber(value);
                double result = Math.Sqrt(value);
                _expression = $"√({originalValue})";
                UpdateExpression(_expression);
                UpdateDisplay(FormatNumber(result));
                _isNewEntry = true;
            }
        }

        private void OnInverse(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                if (value == 0)
                {
                    UpdateDisplay("Ошибка");
                    _isNewEntry = true;
                    return;
                }
                
                string originalValue = FormatNumber(value);
                double result = 1 / value;
                _expression = $"1/({originalValue})";
                UpdateExpression(_expression);
                UpdateDisplay(FormatNumber(result));
                _isNewEntry = true;
            }
        }

        // Memory Functions
        private void OnMemoryClear(object? sender, EventArgs e)
        {
            _memory = 0;
        }

        private void OnMemoryRecall(object? sender, EventArgs e)
        {
            UpdateDisplay(FormatNumber(_memory));
            _isNewEntry = true;
        }

        private void OnMemoryAdd(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                _memory += value;
            }
        }

        private void OnMemorySubtract(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                _memory -= value;
            }
        }

        private void OnMemoryStore(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                _memory = value;
            }
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
