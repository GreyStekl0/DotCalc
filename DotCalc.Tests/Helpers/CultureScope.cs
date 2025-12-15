using System.Globalization;

namespace DotCalc.Tests.Helpers
{
    /// <summary>
    /// Временная подмена <see cref="CultureInfo.CurrentCulture"/> и <see cref="CultureInfo.CurrentUICulture"/> для тестов.
    /// </summary>
    /// <remarks>
    /// Нужна, чтобы результаты форматирования чисел/разделитель дробной части были предсказуемыми.
    /// </remarks>
    internal sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUiCulture;

        public CultureScope(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);

            // Запоминаем текущие культуры, чтобы вернуть их в Dispose().
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUiCulture = CultureInfo.CurrentUICulture;

            // Устанавливаем культуры для текущего потока.
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            // Восстанавливаем исходные значения.
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
