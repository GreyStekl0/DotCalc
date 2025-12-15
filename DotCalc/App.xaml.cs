namespace DotCalc
{
    /// <summary>
    /// Корневой класс приложения MAUI.
    /// </summary>
    public partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Создает главное окно и задает корневую навигацию (Shell).
        /// </summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
