namespace DotCalc.Data
{
    /// <summary>
    /// Константы и настройки для файла базы данных приложения.
    /// </summary>
    public static class DatabaseConstants
    {
        /// <summary>
        /// Имя файла SQLite, который хранится в директории данных приложения.
        /// </summary>
        public const string DatabaseFilename = "DotCalc.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // Открываем базу в режиме чтения/записи.
            SQLite.SQLiteOpenFlags.ReadWrite |
            // Создаем файл базы, если его еще нет.
            SQLite.SQLiteOpenFlags.Create |
            // Разрешаем доступ из нескольких потоков (shared cache).
            SQLite.SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Полный путь к файлу базы данных в app-data директории платформы.
        /// </summary>
        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }
}
