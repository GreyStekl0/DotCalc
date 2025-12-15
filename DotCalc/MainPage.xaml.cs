using System.Collections.ObjectModel;
using DotCalc.Data;
using DotCalc.Models;
using DotCalc.Services;

namespace DotCalc
{
    /// <summary>
    /// Главная страница приложения: калькулятор + панель "Журнал/Память".
    /// </summary>
    /// <remarks>
    /// UI построен на XAML, а логика вычислений вынесена в <see cref="CalculatorEngine"/>.
    /// "Память" хранится в SQLite через <see cref="MemoryDatabase"/>.
    /// </remarks>
    public partial class MainPage
    {
        // Движок калькулятора (чистая логика без привязки к UI).
        private readonly CalculatorEngine _calculator = new();

        /// <summary>
        /// История вычислений (используется как ItemsSource для списка "Журнал").
        /// </summary>
        public ObservableCollection<HistoryItem> History => _calculator.History;
        
        /// <summary>
        /// Список значений "памяти" (используется как ItemsSource для списка "Память").
        /// </summary>
        public ObservableCollection<MemoryItem> MemoryList { get; } = [];
        
        // Текущий активный элемент памяти (для hover эффекта)
        private MemoryItem? _currentHoveredMemoryItem;
        
        // База данных для "памяти" (SQLite файл лежит в AppData).
        private readonly MemoryDatabase _memoryDb = new(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
        
        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            SyncCalculatorToUi();
            
            // Загружаем память из БД при запуске
            Loaded += OnPageLoaded;
        }

        // Событие Loaded удобно тем, что страница уже создана, и можно безопасно трогать UI/привязки.
        private async void OnPageLoaded(object? sender, EventArgs e)
        {
            await LoadMemoryFromDatabaseAsync();
        }

        // Считываем сохраненную память из SQLite и наполняем ObservableCollection для UI.
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
                // Не падаем из-за БД: приложение должно продолжать работать.
                System.Diagnostics.Debug.WriteLine($"Error loading memory: {ex.Message}");
            }
        }

        // Простой "мост" между движком и элементами UI (Label'ами).
        private void SyncCalculatorToUi()
        {
            DisplayLabel.Text = _calculator.DisplayText;
            ExpressionLabel.Text = _calculator.ExpressionText;
        }

        // Переключение вкладок "Журнал" / "Память".
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

        // Одна кнопка "очистить" для обеих вкладок: действие зависит от активной панели.
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

        // Цифры.
        private void OnDigit(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;
            _calculator.Digit(button.Text);
            SyncCalculatorToUi();
        }

        // Десятичный разделитель.
        private void OnDecimal(object? sender, EventArgs e)
        {
            _calculator.Decimal();
            SyncCalculatorToUi();
        }

        // Операторы +, −, ×, ÷.
        private void OnOperator(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;
            _calculator.Operator(button.Text);
            SyncCalculatorToUi();
        }

        // "=".
        private void OnEquals(object? sender, EventArgs e)
        {
            _calculator.Equals();
            SyncCalculatorToUi();
        }

        // Выбор строки истории: переносим выражение/результат на дисплей.
        private void OnHistoryItemSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not HistoryItem selectedItem) return;
            _calculator.SelectHistoryItem(selectedItem);
            SyncCalculatorToUi();
                
            // Сбрасываем выделение
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        }

        // Hover эффекты для элементов памяти (актуально для десктопа).
        private void OnMemoryItemPointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is not Grid { BindingContext: MemoryItem item }) return;
            // Снимаем hover с предыдущего элемента
            if (_currentHoveredMemoryItem != null && _currentHoveredMemoryItem != item)
            {
                _currentHoveredMemoryItem.IsHovered = false;
            }
                
            item.IsHovered = true;
            _currentHoveredMemoryItem = item;
        }

        // Клик по элементу памяти: используем его значение как новый ввод калькулятора.
        private void OnMemoryItemTapped(object? sender, TappedEventArgs e)
        {
            if (e.Parameter is not MemoryItem item)
            {
                return;
            }

            _calculator.SetDisplayText(item.DisplayValue, isNewEntry: true);
            SyncCalculatorToUi();
        }

        // Убираем подсветку при уходе курсора.
        private void OnMemoryItemPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is not Grid grid || grid.BindingContext is not MemoryItem item) return;
            item.IsHovered = false;
            if (_currentHoveredMemoryItem == item)
            {
                _currentHoveredMemoryItem = null;
            }
        }

        private void OnClearHistory(object? sender, EventArgs e)
        {
            _calculator.ClearHistory();
        }

        // "C" — полный сброс.
        private void OnClear(object? sender, EventArgs e)
        {
            _calculator.Clear();
            SyncCalculatorToUi();
        }

        // "CE" — сброс только текущего ввода.
        private void OnClearEntry(object? sender, EventArgs e)
        {
            _calculator.ClearEntry();
            SyncCalculatorToUi();
        }

        // "⌫" — удалить последний символ.
        private void OnBackspace(object? sender, EventArgs e)
        {
            _calculator.Backspace();
            SyncCalculatorToUi();
        }

        // "+/−" — смена знака.
        private void OnNegate(object? sender, EventArgs e)
        {
            _calculator.Negate();
            SyncCalculatorToUi();
        }

        // "%" — проценты.
        private void OnPercent(object? sender, EventArgs e)
        {
            _calculator.Percent();
            SyncCalculatorToUi();
        }

        // "x²" — квадрат.
        private void OnSquare(object? sender, EventArgs e)
        {
            _calculator.Square();
            SyncCalculatorToUi();
        }

        // "√x" — квадратный корень.
        private void OnSquareRoot(object? sender, EventArgs e)
        {
            _calculator.SquareRoot();
            SyncCalculatorToUi();
        }

        // "1/x" — обратное значение.
        private void OnInverse(object? sender, EventArgs e)
        {
            _calculator.Inverse();
            SyncCalculatorToUi();
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
            if (MemoryList.Count <= 0) return;
            var lastItem = MemoryList[0];
            _calculator.SetDisplayText(lastItem.DisplayValue, isNewEntry: true);
            SyncCalculatorToUi();
        }

        /// <summary>
        /// M+ - добавляет текущее значение к последнему элементу в памяти
        /// </summary>
        private async void OnMemoryAdd(object? sender, EventArgs e)
        {
            if (!double.TryParse(_calculator.DisplayText, out double value)) return;
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

        /// <summary>
        /// M- - вычитает текущее значение из последнего элемента в памяти
        /// </summary>
        private async void OnMemorySubtract(object? sender, EventArgs e)
        {
            if (!double.TryParse(_calculator.DisplayText, out double value)) return;
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

        /// <summary>
        /// MS - сохраняет текущее значение в память (новый элемент)
        /// </summary>
        private async void OnMemoryStore(object? sender, EventArgs e)
        {
            if (double.TryParse(_calculator.DisplayText, out double value))
            {
                await AddMemoryItemAsync(value);
            }
        }

        private async Task AddMemoryItemAsync(double value)
        {
            // Сдвигаем порядок существующих элементов
            await _memoryDb.IncrementOrderForAllAsync();

            // Создаём новую запись в БД
            var newEntity = new MemoryItemEntity
            {
                Value = value,
                Order = 0
            };
            await _memoryDb.InsertAsync(newEntity);

            // Добавляем в UI
            MemoryList.Insert(0, new MemoryItem
            {
                DatabaseId = newEntity.Id,
                Value = value
            });
        }

        // Записываем изменения в БД для элемента, который уже существует.
        private async Task UpdateMemoryItemInDatabaseAsync(MemoryItem item)
        {
            var existingEntity = await _memoryDb.GetByIdAsync(item.DatabaseId);
            if (existingEntity is null)
            {
                return;
            }

            existingEntity.Value = item.Value;
            await _memoryDb.UpdateAsync(existingEntity);
        }

        // Memory Item Actions (для кнопок на отдельных элементах памяти)
        
        /// <summary>
        /// MC на отдельном элементе - удаляет только этот элемент
        /// </summary>
        private async void OnMemoryItemClear(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;
            var item = GetMemoryItemFromButton(button);
            if (item != null)
            {
                await DeleteMemoryItemAsync(item);
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
            if (sender is not Button button) return;
            var item = GetMemoryItemFromButton(button);
            if (item == null || !double.TryParse(_calculator.DisplayText, out var value)) return;
            item.Value += value;
            await UpdateMemoryItemInDatabaseAsync(item);
        }

        /// <summary>
        /// M- на отдельном элементе - вычитает текущее значение из этого элемента
        /// </summary>
        private async void OnMemoryItemSubtract(object? sender, EventArgs e)
        {
            if (sender is not Button button) return;
            var item = GetMemoryItemFromButton(button);
            if (item == null || !double.TryParse(_calculator.DisplayText, out var value)) return;
            item.Value -= value;
            await UpdateMemoryItemInDatabaseAsync(item);
        }

        private static MemoryItem? GetMemoryItemFromButton(Button button)
        {
            // Поднимаемся по визуальному дереву: Button -> HorizontalStackLayout -> Grid
            return button.Parent is HorizontalStackLayout { Parent: Grid { BindingContext: MemoryItem item } } ? item : null;
        }

    }
}
