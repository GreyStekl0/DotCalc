namespace DotCalc.Models
{
    /// <summary>
    /// Одна строка истории вычислений (выражение и результат).
    /// </summary>
    public class HistoryItem
    {
        /// <summary>
        /// Текст выражения, например: <c>2 + 3 =</c>.
        /// </summary>
        public string Expression { get; set; } = string.Empty;

        /// <summary>
        /// Результат вычисления в виде строки, уже готовой для показа на дисплее.
        /// </summary>
        public string Result { get; set; } = string.Empty;
    }
}
