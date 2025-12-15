using SQLite;

namespace DotCalc.Data
{
    /// <summary>
    /// Сущность для хранения элемента памяти в БД
    /// </summary>
    public class MemoryItemEntity
    {
        /// <summary>
        /// Первичный ключ записи (автоинкремент).
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        /// <summary>
        /// Числовое значение, сохраненное в памяти.
        /// </summary>
        public double Value { get; set; }
    }
}
