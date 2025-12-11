using System.Collections.ObjectModel;
using DotCalc.Data;
using DotCalc.Models;

namespace DotCalc
{
    public partial class MainPage : ContentPage
    {
        private double _currentValue = 0;
        private double _storedValue = 0;
        private string _currentOperator = "";
        private bool _isNewEntry = true;
        private string _expression = "";
        
        // Для повторения последней операции при нажатии =
        private string _lastOperator = "";
        private double _lastOperand = 0;
        private bool _justCalculated = false;

        // История вычислений
        public ObservableCollection<HistoryItem> History { get; } = [];
        
        // Память (список значений)
        public ObservableCollection<MemoryItem> MemoryList { get; } = [];
        
        // Текущий активный элемент памяти (для hover эффекта)
        private MemoryItem? _currentHoveredMemoryItem;
        
        // База данных для памяти
        private readonly MemoryDatabase _memoryDb = new();
        
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            
            // Загружаем память из БД при запуске
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object? sender, EventArgs e)
        {
            await LoadMemoryFromDatabaseAsync();
        }

        private async Task LoadMemoryFromDatabaseAsync()
        {
            try
            {
                var entities = await _memoryDb.GetAllAsync();
                MemoryList.Clear();
                
                foreach (var entity in entities)
                {
                    MemoryList.Add(new MemoryItem
                    {
                        DatabaseId = entity.Id,
                        Value = entity.Value
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading memory: {ex.Message}");
            }
        }

        private void UpdateDisplay(string value)
        {
            DisplayLabel.Text = value;
        }

        private void UpdateExpression(string expression)
        {
            ExpressionLabel.Text = expression;
        }

        // Tab switching
        private void OnHistoryTabClicked(object? sender, EventArgs e)
        {
            HistoryContent.IsVisible = true;
            MemoryContent.IsVisible = false;
            
            HistoryTabButton.Style = (Style)Resources["TabButtonActiveStyle"];
            MemoryTabButton.Style = (Style)Resources["TabButtonStyle"];
            
            HistoryTabIndicator.HorizontalOptions = LayoutOptions.Start;
            ClearPanelButton.Text = "🗑 Очистить журнал";
        }

        private void OnMemoryTabClicked(object? sender, EventArgs e)
        {
            HistoryContent.IsVisible = false;
            MemoryContent.IsVisible = true;
            
            HistoryTabButton.Style = (Style)Resources["TabButtonStyle"];
            MemoryTabButton.Style = (Style)Resources["TabButtonActiveStyle"];
            
            HistoryTabIndicator.HorizontalOptions = LayoutOptions.End;
            ClearPanelButton.Text = "🗑 Очистить память";
        }

        private async void OnClearPanel(object? sender, EventArgs e)
        {
            if (HistoryContent.IsVisible)
            {
                OnClearHistory(sender, e);
            }
            else
            {
                await ClearAllMemoryAsync();
            }
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
                _justCalculated = false;
            }
        }

        private void OnEquals(object? sender, EventArgs e)
        {
            if (_justCalculated && !string.IsNullOrEmpty(_lastOperator))
            {
                // Повторяем последнюю операцию: результат становится первым операндом
                _storedValue = double.TryParse(DisplayLabel.Text, out double val) ? val : 0;
                
                _expression = FormatNumber(_storedValue) + " " + _lastOperator + " " + FormatNumber(_lastOperand) + " =";
                UpdateExpression(_expression);
                
                // Выполняем вычисление с сохранённым операндом
                double result = CalculateResult(_storedValue, _lastOperator, _lastOperand);
                
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    UpdateDisplay("Ошибка");
                    _isNewEntry = true;
                    _justCalculated = false;
                    return;
                }
                
                string resultStr = FormatNumber(result);
                UpdateDisplay(resultStr);
                
                // Добавляем в историю
                AddToHistory(_expression, resultStr);
                
                _isNewEntry = true;
            }
            else if (!string.IsNullOrEmpty(_currentOperator))
            {
                _currentValue = double.TryParse(DisplayLabel.Text, out double val) ? val : 0;
                
                // Сохраняем операцию и операнд для повторения
                _lastOperator = _currentOperator;
                _lastOperand = _currentValue;
                
                _expression = FormatNumber(_storedValue) + " " + _currentOperator + " " + FormatNumber(_currentValue) + " =";
                UpdateExpression(_expression);
                
                // Выполняем вычисление
                double result = CalculateResult(_storedValue, _currentOperator, _currentValue);
                
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    UpdateDisplay("Ошибка");
                    _isNewEntry = true;
                    _currentOperator = "";
                    _justCalculated = false;
                    return;
                }
                
                _storedValue = result;
                string resultStr = FormatNumber(result);
                UpdateDisplay(resultStr);
                
                // Добавляем в историю
                AddToHistory(_expression, resultStr);
                
                _currentOperator = "";
                _isNewEntry = true;
                _justCalculated = true;
            }
        }

        private void AddToHistory(string expression, string result)
        {
            History.Insert(0, new HistoryItem
            {
                Expression = expression,
                Result = result
            });
        }

        private void OnHistoryItemSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is HistoryItem selectedItem)
            {
                // Восстанавливаем выражение и результат
                UpdateExpression(selectedItem.Expression);
                UpdateDisplay(selectedItem.Result);
                
                // Пытаемся распарсить результат для дальнейших вычислений
                if (double.TryParse(selectedItem.Result, out double result))
                {
                    _storedValue = result;
                }
                
                _isNewEntry = true;
                _currentOperator = "";
                _justCalculated = false;
                
                // Сбрасываем выделение
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }

        private void OnMemoryItemSelected(object? sender, SelectionChangedEventArgs e)
        {
            // Больше не используется для hover, но оставим для выбора значения по клику
            if (e.CurrentSelection.FirstOrDefault() is MemoryItem selectedItem)
            {
                // Устанавливаем значение на дисплей
                UpdateDisplay(selectedItem.DisplayValue);
                _isNewEntry = true;
                
                // Сбрасываем выделение
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }

        private void OnMemoryItemPointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is MemoryItem item)
            {
                // Снимаем hover с предыдущего элемента
                if (_currentHoveredMemoryItem != null && _currentHoveredMemoryItem != item)
                {
                    _currentHoveredMemoryItem.IsHovered = false;
                }
                
                item.IsHovered = true;
                _currentHoveredMemoryItem = item;
            }
        }

        private void OnMemoryItemPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is MemoryItem item)
            {
                item.IsHovered = false;
                if (_currentHoveredMemoryItem == item)
                {
                    _currentHoveredMemoryItem = null;
                }
            }
        }

        private void OnClearHistory(object? sender, EventArgs e)
        {
            History.Clear();
        }

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

        private void PerformCalculation()
        {
            _currentValue = double.TryParse(DisplayLabel.Text, out double val) ? val : 0;
            
            double result = CalculateResult(_storedValue, _currentOperator, _currentValue);
            
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
            _lastOperator = "";
            _lastOperand = 0;
            _justCalculated = false;
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

        // Memory Functions - работа со списком памяти с сохранением в БД
        
        /// <summary>
        /// MC - очищает всю память (все элементы)
        /// </summary>
        private async void OnMemoryClear(object? sender, EventArgs e)
        {
            await ClearAllMemoryAsync();
        }

        private async Task ClearAllMemoryAsync()
        {
            await _memoryDb.DeleteAllAsync();
            MemoryList.Clear();
            _currentHoveredMemoryItem = null;
        }

        /// <summary>
        /// MR - вызывает последний (верхний) элемент из памяти
        /// </summary>
        private void OnMemoryRecall(object? sender, EventArgs e)
        {
            if (MemoryList.Count > 0)
            {
                var lastItem = MemoryList[0];
                UpdateDisplay(lastItem.DisplayValue);
                _isNewEntry = true;
            }
        }

        /// <summary>
        /// M+ - добавляет текущее значение к последнему элементу в памяти
        /// </summary>
        private async void OnMemoryAdd(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                if (MemoryList.Count > 0)
                {
                    var item = MemoryList[0];
                    item.Value += value;
                    await UpdateMemoryItemInDatabaseAsync(item);
                }
                else
                {
                    // Если память пуста, создаём новый элемент
                    await AddMemoryItemAsync(value);
                }
            }
        }

        /// <summary>
        /// M- - вычитает текущее значение из последнего элемента в памяти
        /// </summary>
        private async void OnMemorySubtract(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                if (MemoryList.Count > 0)
                {
                    var item = MemoryList[0];
                    item.Value -= value;
                    await UpdateMemoryItemInDatabaseAsync(item);
                }
                else
                {
                    // Если память пуста, создаём новый элемент с отрицательным значением
                    await AddMemoryItemAsync(-value);
                }
            }
        }

        /// <summary>
        /// MS - сохраняет текущее значение в память (новый элемент)
        /// </summary>
        private async void OnMemoryStore(object? sender, EventArgs e)
        {
            if (double.TryParse(DisplayLabel.Text, out double value))
            {
                await AddMemoryItemAsync(value);
            }
        }

        private async Task AddMemoryItemAsync(double value)
        {
            // Сдвигаем порядок существующих элементов
            var allEntities = await _memoryDb.GetAllAsync();
            foreach (var entity in allEntities)
            {
                entity.Order++;
                await _memoryDb.UpdateAsync(entity);
            }

            // Создаём новую запись в БД
            var newEntity = new MemoryItemEntity
            {
                Value = value,
                Order = 0,
                CreatedAt = DateTime.Now
            };
            await _memoryDb.InsertAsync(newEntity);

            // Добавляем в UI
            MemoryList.Insert(0, new MemoryItem
            {
                DatabaseId = newEntity.Id,
                Value = value
            });
        }

        private async Task UpdateMemoryItemInDatabaseAsync(MemoryItem item)
        {
            var entity = new MemoryItemEntity
            {
                Id = item.DatabaseId,
                Value = item.Value
            };
            
            // Получаем существующую запись для сохранения Order
            var allEntities = await _memoryDb.GetAllAsync();
            var existingEntity = allEntities.FirstOrDefault(e => e.Id == item.DatabaseId);
            if (existingEntity != null)
            {
                entity.Order = existingEntity.Order;
                entity.CreatedAt = existingEntity.CreatedAt;
            }
            
            await _memoryDb.UpdateAsync(entity);
        }

        // Memory Item Actions (для кнопок на отдельных элементах памяти)
        
        /// <summary>
        /// MC на отдельном элементе - удаляет только этот элемент
        /// </summary>
        private async void OnMemoryItemClear(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var item = GetMemoryItemFromButton(button);
                if (item != null)
                {
                    await DeleteMemoryItemAsync(item);
                }
            }
        }

        private async Task DeleteMemoryItemAsync(MemoryItem item)
        {
            // Удаляем из БД
            var entity = new MemoryItemEntity { Id = item.DatabaseId };
            await _memoryDb.DeleteAsync(entity);

            // Удаляем из UI
            if (_currentHoveredMemoryItem == item)
            {
                _currentHoveredMemoryItem = null;
            }
            MemoryList.Remove(item);
        }

        /// <summary>
        /// M+ на отдельном элементе - добавляет текущее значение к этому элементу
        /// </summary>
        private async void OnMemoryItemAdd(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var item = GetMemoryItemFromButton(button);
                if (item != null && double.TryParse(DisplayLabel.Text, out double value))
                {
                    item.Value += value;
                    await UpdateMemoryItemInDatabaseAsync(item);
                }
            }
        }

        /// <summary>
        /// M- на отдельном элементе - вычитает текущее значение из этого элемента
        /// </summary>
        private async void OnMemoryItemSubtract(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                var item = GetMemoryItemFromButton(button);
                if (item != null && double.TryParse(DisplayLabel.Text, out double value))
                {
                    item.Value -= value;
                    await UpdateMemoryItemInDatabaseAsync(item);
                }
            }
        }

        private static MemoryItem? GetMemoryItemFromButton(Button button)
        {
            // Поднимаемся по визуальному дереву: Button -> HorizontalStackLayout -> Grid
            if (button.Parent is HorizontalStackLayout stackLayout && 
                stackLayout.Parent is Grid grid && 
                grid.BindingContext is MemoryItem item)
            {
                return item;
            }
            return null;
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
