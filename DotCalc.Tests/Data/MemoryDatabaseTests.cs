using DotCalc.Data;
using SQLite;
using SQLitePCL;
using Xunit;

namespace DotCalc.Tests.Data
{
    /// <summary>
    /// Интеграционные тесты для <see cref="MemoryDatabase"/> (работа с реальным SQLite-файлом во временной папке).
    /// </summary>
    public class MemoryDatabaseTests
    {
        // Для тестов используем те же флаги открытия, что и в приложении.
        private const SQLiteOpenFlags TestFlags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        static MemoryDatabaseTests()
        {
            // Инициализация SQLitePCL (нужна для работы sqlite-net-pcl в тестовом окружении).
            Batteries_V2.Init();
        }

        // Создаем отдельную базу на каждый тест, чтобы они не влияли друг на друга.
        private static MemoryDatabase CreateDatabase()
        {
            var directory = Path.Combine(Path.GetTempPath(), "DotCalc.Tests");
            Directory.CreateDirectory(directory);

            var databasePath = Path.Combine(directory, $"{Guid.NewGuid():N}.db3");
            return new MemoryDatabase(databasePath, TestFlags);
        }

        [Fact]
        public async Task InsertAndGetById_Works()
        {
            var db = CreateDatabase();
            var entity = new MemoryItemEntity
            {
                Value = 123
            };

            var inserted = await db.InsertAsync(entity);

            Assert.Equal(1, inserted);
            Assert.NotEqual(0, entity.Id);

            var loaded = await db.GetByIdAsync(entity.Id);

            Assert.NotNull(loaded);
            Assert.Equal(entity.Id, loaded.Id);
            Assert.Equal(123, loaded.Value);
        }

        [Fact]
        public async Task GetAllAsync_OrdersByIdDescending()
        {
            var db = CreateDatabase();

            await db.InsertAsync(new MemoryItemEntity { Value = 1 });
            await db.InsertAsync(new MemoryItemEntity { Value = 2 });

            var all = await db.GetAllAsync();

            Assert.Equal(2, all.Count);
            // Новые первые (Id по убыванию)
            Assert.Equal(2, all[0].Value);
            Assert.Equal(1, all[1].Value);
        }

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            var db = CreateDatabase();
            var entity = new MemoryItemEntity { Value = 1 };
            await db.InsertAsync(entity);

            entity.Value = 42;
            var updated = await db.UpdateAsync(entity);

            Assert.Equal(1, updated);

            var loaded = await db.GetByIdAsync(entity.Id);

            Assert.NotNull(loaded);
            Assert.Equal(42, loaded.Value);
        }

        [Fact]
        public async Task DeleteAsync_RemovesRow()
        {
            var db = CreateDatabase();
            var entity = new MemoryItemEntity { Value = 1 };
            await db.InsertAsync(entity);

            var deleted = await db.DeleteAsync(entity);

            Assert.Equal(1, deleted);
            Assert.Null(await db.GetByIdAsync(entity.Id));
            Assert.Empty(await db.GetAllAsync());
        }

        [Fact]
        public async Task DeleteAllAsync_RemovesAllRows()
        {
            var db = CreateDatabase();

            await db.InsertAsync(new MemoryItemEntity { Value = 1 });
            await db.InsertAsync(new MemoryItemEntity { Value = 2 });

            var deleted = await db.DeleteAllAsync();

            Assert.Equal(2, deleted);
            Assert.Empty(await db.GetAllAsync());
        }
    }
}
