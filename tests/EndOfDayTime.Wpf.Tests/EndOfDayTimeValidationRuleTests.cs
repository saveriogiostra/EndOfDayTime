using System.Globalization;
using EndOfDayTime.Wpf;
using System.Windows.Controls;
using Xunit;

namespace EndOfDayTime.Wpf.Tests
{
    public class EndOfDayTimeValidationRuleTests
    {
        private readonly EndOfDayTimeValidationRule _rule = new();

        // ── Valid inputs ─────────────────────────────────────────────────

        [Theory]
        [InlineData("00:00")]
        [InlineData("09:00")]
        [InlineData("09:30")]
        [InlineData("23:59")]
        [InlineData("24:00")]
        public void Validate_ValidTime_ReturnsValid(string input)
        {
            var result = _rule.Validate(input, CultureInfo.InvariantCulture);
            Assert.True(result.IsValid);
        }

        // ── Invalid inputs ───────────────────────────────────────────────

        [Theory]
        [InlineData("25:00")]
        [InlineData("24:01")]
        [InlineData("9:00")]
        [InlineData("abc")]
        [InlineData("12:60")]
        public void Validate_InvalidTime_ReturnsInvalid(string input)
        {
            var result = _rule.Validate(input, CultureInfo.InvariantCulture);
            Assert.False(result.IsValid);
            Assert.NotNull(result.ErrorContent);
        }

        // ── Empty/null inputs ────────────────────────────────────────────

        [Fact]
        public void Validate_EmptyString_ReturnsInvalid()
        {
            var result = _rule.Validate(string.Empty, CultureInfo.InvariantCulture);
            Assert.False(result.IsValid);
            Assert.Equal("Time is required.", result.ErrorContent);
        }

        [Fact]
        public void Validate_NullValue_ReturnsInvalid()
        {
            var result = _rule.Validate(null!, CultureInfo.InvariantCulture);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Validate_Whitespace_ReturnsInvalid()
        {
            var result = _rule.Validate("   ", CultureInfo.InvariantCulture);
            Assert.False(result.IsValid);
        }

        // ── Error messages ───────────────────────────────────────────────

        [Fact]
        public void Validate_InvalidTime_ErrorMessageContainsRange()
        {
            var result = _rule.Validate("bad", CultureInfo.InvariantCulture);
            Assert.Contains("00:00–24:00", result.ErrorContent?.ToString());
        }

        [Fact]
        public void Validate_ValidTime_ErrorContentIsNull()
        {
            var result = _rule.Validate("09:00", CultureInfo.InvariantCulture);
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorContent);
        }
    }
}