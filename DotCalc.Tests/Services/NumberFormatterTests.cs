using System.Globalization;
using DotCalc.Services;
using DotCalc.Tests.Helpers;
using Xunit;

namespace DotCalc.Tests.Services
{
    /// <summary>
    /// Тесты для <see cref="NumberFormatter"/>: форматирование целых/дробных/экспоненциальных значений.
    /// </summary>
    public class NumberFormatterTests
    {
        [Fact]
        public void FormatNumber_IntegerUnderThreshold_FormatsWithoutDecimals()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            Assert.Equal("123", NumberFormatter.FormatNumber(123));
            Assert.Equal("-123", NumberFormatter.FormatNumber(-123));
            Assert.Equal("0", NumberFormatter.FormatNumber(0));
        }

        [Fact]
        public void FormatNumber_UsesCurrentCultureDecimalSeparator()
        {
            using var _ = new CultureScope(new CultureInfo("ru-RU"));

            Assert.Equal("1,5", NumberFormatter.FormatNumber(1.5));
        }

        [Fact]
        public void FormatNumber_DoesNotTrimExponentZeros()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            Assert.Equal("1E+20", NumberFormatter.FormatNumber(1e20));
        }
    }
}
