using SQLite;

namespace DotCalc.Data
{
    /// <summary>
    /// Сущность для хранения элемента памяти в БД
    /// </summary>
    public class MemoryItemEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public double Value { get; set; }
        
        /// <summary>
        /// Порядок элемента (для сохранения порядка в списке)
        /// </summary>
        public int Order { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
