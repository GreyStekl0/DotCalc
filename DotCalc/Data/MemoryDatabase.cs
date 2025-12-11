using SQLite;

namespace DotCalc.Data
{
    public class MemoryDatabase
    {
        private SQLiteAsyncConnection? _database;

        private async Task InitAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            await _database.CreateTableAsync<MemoryItemEntity>();
        }

        /// <summary>
        /// Получить все элементы памяти, отсортированные по порядку
        /// </summary>
        public async Task<List<MemoryItemEntity>> GetAllAsync()
        {
            await InitAsync();
            return await _database!.Table<MemoryItemEntity>()
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        /// <summary>
        /// Сохранить новый элемент памяти
        /// </summary>
        public async Task<int> InsertAsync(MemoryItemEntity item)
        {
            await InitAsync();
            return await _database!.InsertAsync(item);
        }

        /// <summary>
        /// Обновить существующий элемент памяти
        /// </summary>
        public async Task<int> UpdateAsync(MemoryItemEntity item)
        {
            await InitAsync();
            return await _database!.UpdateAsync(item);
        }

        /// <summary>
        /// Удалить элемент памяти
        /// </summary>
        public async Task<int> DeleteAsync(MemoryItemEntity item)
        {
            await InitAsync();
            return await _database!.DeleteAsync(item);
        }

        /// <summary>
        /// Удалить все элементы памяти
        /// </summary>
        public async Task<int> DeleteAllAsync()
        {
            await InitAsync();
            return await _database!.DeleteAllAsync<MemoryItemEntity>();
        }

        /// <summary>
        /// Пересчитать порядок всех элементов
        /// </summary>
        public async Task ReorderAsync(List<MemoryItemEntity> items)
        {
            await InitAsync();
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Order = i;
                await _database!.UpdateAsync(items[i]);
            }
        }
    }
}
