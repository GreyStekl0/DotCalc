using Foundation;

namespace DotCalc
{
    [Register("AppDelegate")]
    // AppDelegate для iOS: системная точка входа, которая создает MAUI-приложение.
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
