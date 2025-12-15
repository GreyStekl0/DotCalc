using SQLite;

namespace DotCalc.Data
{
    /// <summary>
    /// Простой слой доступа к SQLite для "памяти" калькулятора.
    /// </summary>
    /// <remarks>
    /// Внутри используется один <see cref="SQLiteAsyncConnection"/> и <see cref="SemaphoreSlim"/> для сериализации операций.
    /// </remarks>
    public class MemoryDatabase
    {
        private readonly string _databasePath;
        private readonly SQLiteOpenFlags _openFlags;
        private SQLiteAsyncConnection? _database;
        private readonly SemaphoreSlim _mutex = new(1, 1);

        /// <summary>
        /// Создает экземпляр доступа к БД.
        /// </summary>
        /// <param name="databasePath">Путь к файлу SQLite.</param>
        /// <param name="openFlags">Флаги открытия (ReadWrite/Create/SharedCache и т.п.).</param>
        public MemoryDatabase(string databasePath, SQLiteOpenFlags openFlags)
        {
            _databasePath = databasePath;
            _openFlags = openFlags;
        }

        // Создает соединение лениво и гарантирует наличие таблицы.
        private async Task<SQLiteAsyncConnection> GetDatabaseLockedAsync()
        {
            if (_database is not null)
            {
                return _database;
            }

            _database = new SQLiteAsyncConnection(_databasePath, _openFlags);
            await _database.CreateTableAsync<MemoryItemEntity>();
            return _database;
        }

        /// <summary>
        /// Получает все элементы памяти, отсортированные по полю <see cref="MemoryItemEntity.Order"/>.
        /// </summary>
        public async Task<List<MemoryItemEntity>> GetAllAsync()
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.Table<MemoryItemEntity>()
                    .OrderBy(x => x.Order)
                    .ToListAsync();
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Ищет запись по идентификатору.
        /// </summary>
        /// <param name="id">ID записи.</param>
        /// <returns>Найденная запись или <c>null</c>.</returns>
        public async Task<MemoryItemEntity?> GetByIdAsync(int id)
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.Table<MemoryItemEntity>()
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Увеличивает поле <see cref="MemoryItemEntity.Order"/> для всех записей на 1.
        /// </summary>
        /// <remarks>
        /// Используется, чтобы вставлять новый элемент в начало списка (Order = 0),
        /// сдвигая остальные вниз.
        /// </remarks>
        /// <returns>Количество обновленных строк.</returns>
        public async Task<int> IncrementOrderForAllAsync()
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.ExecuteAsync("UPDATE MemoryItemEntity SET \"Order\" = \"Order\" + 1");
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Сохраняет новый элемент памяти.
        /// </summary>
        /// <param name="item">Сущность для вставки.</param>
        /// <returns>Количество вставленных строк (обычно 1).</returns>
        public async Task<int> InsertAsync(MemoryItemEntity item)
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.InsertAsync(item);
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Обновляет существующий элемент памяти.
        /// </summary>
        /// <param name="item">Сущность с заполненным ID.</param>
        /// <returns>Количество обновленных строк.</returns>
        public async Task<int> UpdateAsync(MemoryItemEntity item)
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.UpdateAsync(item);
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Удаляет элемент памяти.
        /// </summary>
        /// <param name="item">Сущность с заполненным ID.</param>
        /// <returns>Количество удаленных строк.</returns>
        public async Task<int> DeleteAsync(MemoryItemEntity item)
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.DeleteAsync(item);
            }
            finally
            {
                _mutex.Release();
            }
        }

        /// <summary>
        /// Удаляет все элементы памяти.
        /// </summary>
        /// <returns>Количество удаленных строк.</returns>
        public async Task<int> DeleteAllAsync()
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                return await database.DeleteAllAsync<MemoryItemEntity>();
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
