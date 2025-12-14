using System.Globalization;

namespace DotCalc.Services
{
    public static class NumberFormatter
    {
        public static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
            {
                return value.ToString("0", CultureInfo.CurrentCulture);
            }

            return value.ToString("G15", CultureInfo.CurrentCulture)
                .TrimEnd('0')
                .TrimEnd(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
        }
    }
}
