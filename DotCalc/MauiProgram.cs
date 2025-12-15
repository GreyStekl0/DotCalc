using Microsoft.Extensions.Logging;

namespace DotCalc
{
    /// <summary>
    /// Точка настройки приложения MAUI: DI, шрифты, логирование и т.п.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Создает и настраивает <see cref="MauiApp"/>.
        /// </summary>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    // Подключаем шрифты, чтобы ими можно было пользоваться в XAML.
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		// В Debug выводим логи в отладочную консоль.
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
