using Microsoft.UI.Xaml;

// Справка по WinUI и структуре проекта:
// http://aka.ms/winui-project-info

namespace DotCalc.WinUI
{
    /// <summary>
    /// WinUI-хост приложения: дополняет стандартный класс <c>Application</c> и подключает MAUI.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Инициализирует единственный объект приложения.
        /// Это первая строка "пользовательского" кода, логический аналог <c>main()</c>/<c>WinMain()</c>.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
