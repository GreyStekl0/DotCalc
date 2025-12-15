using System.Globalization;
using DotCalc.Services;
using DotCalc.Tests.Helpers;
using Xunit;

namespace DotCalc.Tests.Services
{
    /// <summary>
    /// Тесты для <see cref="CalculatorEngine"/>: базовые операции, история и граничные случаи.
    /// </summary>
    public class CalculatorEngineTests
    {
        // Бинарные операции и заполнение истории.
        [Fact]
        public void Addition_AddsToHistory()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.Equals();

            Assert.Equal("5", calculator.DisplayText);
            Assert.Equal("2 + 3 =", calculator.ExpressionText);
            Assert.Single(calculator.History);
            Assert.Equal("2 + 3 =", calculator.History[0].Expression);
            Assert.Equal("5", calculator.History[0].Result);
        }

        [Fact]
        public void Equals_WhenRepeated_RepeatsLastOperation()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.Equals();
            calculator.Equals();
            calculator.Equals();

            Assert.Equal("11", calculator.DisplayText);
            Assert.Equal("8 + 3 =", calculator.ExpressionText);
            Assert.Equal(3, calculator.History.Count);
            Assert.Equal("8 + 3 =", calculator.History[0].Expression);
            Assert.Equal("11", calculator.History[0].Result);
            Assert.Equal("5 + 3 =", calculator.History[1].Expression);
            Assert.Equal("8", calculator.History[1].Result);
            Assert.Equal("2 + 3 =", calculator.History[2].Expression);
            Assert.Equal("5", calculator.History[2].Result);
        }

        [Fact]
        public void Operator_WhenChained_PerformsImmediateCalculation()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.Operator("×");
            calculator.Digit("4");
            calculator.Equals();

            Assert.Equal("20", calculator.DisplayText);
            Assert.Equal("5 × 4 =", calculator.ExpressionText);
        }

        // Ошибки (NaN/Infinity) не должны попадать в историю.
        [Fact]
        public void DivisionByZero_ShowsError_AndDoesNotAddToHistory()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("1");
            calculator.Operator("÷");
            calculator.Digit("0");
            calculator.Equals();

            Assert.Equal(CalculatorEngine.ErrorText, calculator.DisplayText);
            Assert.Equal("1 ÷ 0 =", calculator.ExpressionText);
            Assert.Empty(calculator.History);

            calculator.Digit("7");
            Assert.Equal("7", calculator.DisplayText);
        }

        // Проценты.
        [Fact]
        public void Percent_WithoutOperator_DividesBy100()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("5");
            calculator.Digit("0");
            calculator.Percent();

            Assert.Equal("0.5", calculator.DisplayText);
        }

        [Fact]
        public void Percent_WithOperator_UsesStoredValuePercentage()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Digit("0");
            calculator.Digit("0");
            calculator.Operator("+");
            calculator.Digit("1");
            calculator.Digit("0");
            calculator.Percent();
            calculator.Equals();

            Assert.Equal("220", calculator.DisplayText);
            Assert.Equal("200 + 20 =", calculator.ExpressionText);
        }

        // Унарные операции (x², √x, 1/x).
        [Fact]
        public void Square_UpdatesExpressionAndDisplay()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("5");
            calculator.Square();

            Assert.Equal("25", calculator.DisplayText);
            Assert.Equal("sqr(5)", calculator.ExpressionText);
        }

        [Fact]
        public void SquareRoot_OnNegative_ShowsError_WithoutChangingExpression()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("9");
            calculator.Negate();

            var expressionBefore = calculator.ExpressionText;
            calculator.SquareRoot();

            Assert.Equal(CalculatorEngine.ErrorText, calculator.DisplayText);
            Assert.Equal(expressionBefore, calculator.ExpressionText);
        }

        [Fact]
        public void Inverse_OfZero_ShowsError_WithoutChangingExpression()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("0");

            var expressionBefore = calculator.ExpressionText;
            calculator.Inverse();

            Assert.Equal(CalculatorEngine.ErrorText, calculator.DisplayText);
            Assert.Equal(expressionBefore, calculator.ExpressionText);
        }

        // Редактирование текущего ввода.
        [Fact]
        public void Backspace_RemovesDigits_AndResetsToZero()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("1");
            calculator.Digit("2");
            calculator.Digit("3");
            calculator.Backspace();
            calculator.Backspace();
            calculator.Backspace();

            Assert.Equal("0", calculator.DisplayText);

            calculator.Digit("4");
            Assert.Equal("4", calculator.DisplayText);
        }

        [Fact]
        public void Negate_DuringEntry_AllowsContinuingDigits()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("5");
            calculator.Negate();
            calculator.Digit("1");

            Assert.Equal("-51", calculator.DisplayText);
        }

        // Команды сброса.
        [Fact]
        public void ClearEntry_DoesNotClearExpression()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.ClearEntry();

            Assert.Equal("0", calculator.DisplayText);
            Assert.Equal("2 +", calculator.ExpressionText);
        }

        [Fact]
        public void Clear_DoesNotClearHistory()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.Equals();
            calculator.Clear();

            Assert.Equal("0", calculator.DisplayText);
            Assert.Equal(string.Empty, calculator.ExpressionText);
            Assert.Single(calculator.History);
        }

        // Работа с историей и вводом дробной части.
        [Fact]
        public void SelectHistoryItem_LoadsExpressionAndResult_AndStartsNewEntry()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.Equals();

            var item = calculator.History[0];
            calculator.SelectHistoryItem(item);

            Assert.Equal(item.Expression, calculator.ExpressionText);
            Assert.Equal(item.Result, calculator.DisplayText);

            calculator.Digit("7");
            Assert.Equal("7", calculator.DisplayText);
        }

        [Fact]
        public void Decimal_AddsSeparatorOnce_WithLeadingZero()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Decimal();
            calculator.Decimal();
            calculator.Digit("5");

            Assert.Equal("0.5", calculator.DisplayText);
        }

        [Fact]
        public void SquareRoot_OnPositive_UpdatesExpressionAndDisplay()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("9");
            calculator.SquareRoot();

            Assert.Equal("3", calculator.DisplayText);
            Assert.Equal("√(9)", calculator.ExpressionText);
        }

        [Fact]
        public void Inverse_OnNonZero_UpdatesExpressionAndDisplay()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("4");
            calculator.Inverse();

            Assert.Equal("0.25", calculator.DisplayText);
            Assert.Equal("1/(4)", calculator.ExpressionText);
        }

        // Очистка истории.
        [Fact]
        public void ClearHistory_RemovesAllItems()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var calculator = new CalculatorEngine();
            calculator.Digit("2");
            calculator.Operator("+");
            calculator.Digit("3");
            calculator.Equals();

            calculator.ClearHistory();

            Assert.Empty(calculator.History);
        }
    }
}
