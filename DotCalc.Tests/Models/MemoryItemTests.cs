using System.Globalization;
using DotCalc.Models;
using DotCalc.Tests.Helpers;
using Xunit;

namespace DotCalc.Tests.Models
{
    /// <summary>
    /// Тесты для <see cref="MemoryItem"/>: уведомления <see cref="System.ComponentModel.INotifyPropertyChanged"/> и форматирование.
    /// </summary>
    public class MemoryItemTests
    {
        [Fact]
        public void Value_WhenChanged_RaisesPropertyChangedForValueAndDisplayValue()
        {
            using var _ = new CultureScope(new CultureInfo("en-US"));

            var item = new MemoryItem();
            List<string?> changedProperties = [];
            item.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

            item.Value = 1.5;

            Assert.Contains(nameof(MemoryItem.Value), changedProperties);
            Assert.Contains(nameof(MemoryItem.DisplayValue), changedProperties);
            Assert.Equal("1.5", item.DisplayValue);
        }

        [Fact]
        public void Value_WhenSetToSame_DoesNotRaisePropertyChanged()
        {
            var item = new MemoryItem();
            item.Value = 1;

            List<string?> changedProperties = [];
            item.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

            item.Value = 1;

            Assert.Empty(changedProperties);
        }

        [Fact]
        public void IsHovered_WhenChanged_RaisesPropertyChanged()
        {
            var item = new MemoryItem();
            List<string?> changedProperties = [];
            item.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

            item.IsHovered = true;

            Assert.Equal([nameof(MemoryItem.IsHovered)], changedProperties);
        }
    }
}
