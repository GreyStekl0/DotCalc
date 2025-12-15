using Android.App;
using Android.Runtime;

namespace DotCalc
{
    [Application]
    // Класс Application для Android: точка инициализации MAUI на уровне процесса.
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        // Подключаем общую конфигурацию приложения из MauiProgram.
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
