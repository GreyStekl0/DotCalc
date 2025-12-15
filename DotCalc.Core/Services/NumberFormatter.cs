using System.Globalization;

namespace DotCalc.Services
{
    /// <summary>
    /// Утилита для форматирования чисел так, как их удобно показывать в UI калькулятора.
    /// </summary>
    public static class NumberFormatter
    {
        /// <summary>
        /// Преобразует число в строку с учетом текущей культуры (разделитель дробной части),
        /// убирая лишние нули после запятой/точки и сохраняя экспоненциальную форму при необходимости.
        /// </summary>
        public static string FormatNumber(double value)
        {
            var culture = CultureInfo.CurrentCulture;

            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
            {
                return value.ToString("0", culture);
            }

            var formatted = value.ToString("G15", culture);

            var exponentIndex = formatted.IndexOf('E');
            if (exponentIndex < 0)
            {
                exponentIndex = formatted.IndexOf('e');
            }

            if (exponentIndex < 0)
            {
                return TrimTrailingZerosAfterDecimal(formatted, culture);
            }

            var mantissa = formatted[..exponentIndex];
            var exponent = formatted[exponentIndex..];
            mantissa = TrimTrailingZerosAfterDecimal(mantissa, culture);
            return mantissa + exponent;
        }

        private static string TrimTrailingZerosAfterDecimal(string text, CultureInfo culture)
        {
            var separator = culture.NumberFormat.NumberDecimalSeparator;

            // Если дробной части нет — ничего "подрезать" не нужно.
            if (!text.Contains(separator, StringComparison.Ordinal))
            {
                return text;
            }

            // Убираем нули справа и возможный "висящий" разделитель.
            text = text.TrimEnd('0');
            if (text.EndsWith(separator, StringComparison.Ordinal))
            {
                text = text[..^separator.Length];
            }

            return text;
        }
    }
}
