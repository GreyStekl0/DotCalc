using SQLite;

namespace DotCalc.Data
{
    public class MemoryDatabase
    {
        private SQLiteAsyncConnection? _database;
        private readonly SemaphoreSlim _mutex = new(1, 1);

        private async Task<SQLiteAsyncConnection> GetDatabaseLockedAsync()
        {
            if (_database is not null)
            {
                return _database;
            }

            _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            await _database.CreateTableAsync<MemoryItemEntity>();
            return _database;
        }

        /// <summary>
        /// Получить все элементы памяти, отсортированные по порядку
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
        /// Сохранить новый элемент памяти
        /// </summary>
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
        /// Обновить существующий элемент памяти
        /// </summary>
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
        /// Удалить элемент памяти
        /// </summary>
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
        /// Удалить все элементы памяти
        /// </summary>
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

        /// <summary>
        /// Пересчитать порядок всех элементов
        /// </summary>
        public async Task ReorderAsync(List<MemoryItemEntity> items)
        {
            await _mutex.WaitAsync();
            try
            {
                var database = await GetDatabaseLockedAsync();
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Order = i;
                    await database.UpdateAsync(items[i]);
                }
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}
