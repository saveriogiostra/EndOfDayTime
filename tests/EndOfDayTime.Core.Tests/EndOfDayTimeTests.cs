using System;
using System.Text.Json;
using System.ComponentModel;
using EndOfDayTime.Core;
using Xunit;

namespace EndOfDayTime.Core.Tests
{
    public class EndOfDayTimeTests
    {
        // ── Construction ────────────────────────────────────────────────

        [Fact]
        public void Constructor_ValidTime_CreatesInstance()
        {
            var t = new EndOfDayTime(9, 30);
            Assert.Equal(9, t.Hour);
            Assert.Equal(30, t.Minute);
        }

        [Fact]
        public void Constructor_EndOfDay_CreatesInstance()
        {
            var t = new EndOfDayTime(24, 0);
            Assert.True(t.IsEndOfDay);
            Assert.Equal(1440, t.TotalMinutes);
        }

        [Fact]
        public void Constructor_InvalidHour_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EndOfDayTime(25, 0));
        }

        [Fact]
        public void Constructor_InvalidMinute_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EndOfDayTime(9, 60));
        }

        [Fact]
        public void Constructor_Hour24MinuteNonZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EndOfDayTime(24, 1));
        }

        // ── Parse ────────────────────────────────────────────────────────

        [Theory]
        [InlineData("00:00", 0, 0)]
        [InlineData("09:00", 9, 0)]
        [InlineData("09:30", 9, 30)]
        [InlineData("23:59", 23, 59)]
        [InlineData("24:00", 24, 0)]
        public void Parse_ValidStrings_ReturnsCorrectTime(string input, int expectedHour, int expectedMinute)
        {
            var t = EndOfDayTime.Parse(input);
            Assert.Equal(expectedHour, t.Hour);
            Assert.Equal(expectedMinute, t.Minute);
        }

        [Theory]
        [InlineData("25:00")]
        [InlineData("24:01")]
        [InlineData("9:00")]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("12:60")]
        public void Parse_InvalidStrings_Throws(string input)
        {
            Assert.Throws<FormatException>(() => EndOfDayTime.Parse(input));
        }

        [Fact]
        public void TryParse_ValidString_ReturnsTrueAndResult()
        {
            var success = EndOfDayTime.TryParse("24:00", out var result);
            Assert.True(success);
            Assert.True(result.IsEndOfDay);
        }

        [Fact]
        public void TryParse_InvalidString_ReturnsFalse()
        {
            var success = EndOfDayTime.TryParse("bad", out var result);
            Assert.False(success);
            Assert.Equal(default, result);
        }

        // ── ToString ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(0, 0, "00:00")]
        [InlineData(9, 0, "09:00")]
        [InlineData(9, 5, "09:05")]
        [InlineData(23, 59, "23:59")]
        [InlineData(24, 0, "24:00")]
        public void ToString_ReturnsCorrectFormat(int hour, int minute, string expected)
        {
            var t = new EndOfDayTime(hour, minute);
            Assert.Equal(expected, t.ToString());
        }

        // ── Equality ─────────────────────────────────────────────────────

        [Fact]
        public void Equality_SameValues_AreEqual()
        {
            var a = new EndOfDayTime(9, 30);
            var b = new EndOfDayTime(9, 30);
            Assert.Equal(a, b);
            Assert.True(a == b);
        }

        [Fact]
        public void Equality_DifferentValues_AreNotEqual()
        {
            var a = new EndOfDayTime(9, 30);
            var b = new EndOfDayTime(10, 0);
            Assert.NotEqual(a, b);
            Assert.True(a != b);
        }

        // ── Comparison ───────────────────────────────────────────────────

        [Fact]
        public void Comparison_EarlierTimeLessThanLater()
        {
            var a = new EndOfDayTime(9, 0);
            var b = new EndOfDayTime(24, 0);
            Assert.True(a < b);
            Assert.True(b > a);
        }

        [Fact]
        public void Comparison_EndOfDayIsGreaterThanAllOthers()
        {
            var eod = EndOfDayTime.EndOfDay;
            var last = new EndOfDayTime(23, 59);
            Assert.True(eod > last);
        }

        // ── Subtraction ──────────────────────────────────────────────────

        [Fact]
        public void Subtraction_ReturnsCorrectTimeSpan()
        {
            var start = new EndOfDayTime(9, 0);
            var end = new EndOfDayTime(17, 30);
            var duration = end - start;
            Assert.Equal(TimeSpan.FromHours(8.5), duration);
        }

        [Fact]
        public void Subtraction_EndOfDayMinus0000_Returns24Hours()
        {
            var start = new EndOfDayTime(0, 0);
            var end = EndOfDayTime.EndOfDay;
            var duration = end - start;
            Assert.Equal(TimeSpan.FromHours(24), duration);
        }

        // ── Static values ────────────────────────────────────────────────

        [Fact]
        public void StaticValues_AreCorrect()
        {
            Assert.Equal(0, EndOfDayTime.MinValue.TotalMinutes);
            Assert.Equal(1440, EndOfDayTime.MaxValue.TotalMinutes);
            Assert.True(EndOfDayTime.EndOfDay.IsEndOfDay);
        }

        // ── TypeConverter ────────────────────────────────────────────────

        [Fact]
        public void TypeConverter_FromString_ReturnsCorrectValue()
        {
            var converter = TypeDescriptor.GetConverter(typeof(EndOfDayTime));
            var result = (EndOfDayTime)converter.ConvertFromString("24:00")!;
            Assert.True(result.IsEndOfDay);
        }

        [Fact]
        public void TypeConverter_ToString_ReturnsCorrectString()
        {
            var converter = TypeDescriptor.GetConverter(typeof(EndOfDayTime));
            var t = new EndOfDayTime(24, 0);
            var result = converter.ConvertToString(t);
            Assert.Equal("24:00", result);
        }

        // ── JsonConverter ────────────────────────────────────────────────

        [Fact]
        public void JsonConverter_Deserialize_ReturnsCorrectValue()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new EndOfDayTimeJsonConverter());
            var result = JsonSerializer.Deserialize<EndOfDayTime>("\"24:00\"", options);
            Assert.True(result.IsEndOfDay);
        }

        [Fact]
        public void JsonConverter_Serialize_ReturnsCorrectString()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new EndOfDayTimeJsonConverter());
            var t = new EndOfDayTime(9, 30);
            var json = JsonSerializer.Serialize(t, options);
            Assert.Equal("\"09:30\"", json);
        }

        [Fact]
        public void JsonConverter_RoundTrip_EndOfDay()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new EndOfDayTimeJsonConverter());
            var original = EndOfDayTime.EndOfDay;
            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<EndOfDayTime>(json, options);
            Assert.Equal(original, deserialized);
        }

        private class JsonShift
        {
            public EndOfDayTime Start { get; set; }
            public EndOfDayTime? End { get; set; }
        }

        [Fact]
        public void Json_WithoutOptions_SerializesAsString()
        {
            var json = JsonSerializer.Serialize(new JsonShift { Start = new EndOfDayTime(0, 0), End = EndOfDayTime.EndOfDay });
            Assert.Equal("{\"Start\":\"00:00\",\"End\":\"24:00\"}", json);
        }

        [Fact]
        public void Json_WithoutOptions_DeserializesFromString()
        {
            var shift = JsonSerializer.Deserialize<JsonShift>("{\"Start\":\"09:30\",\"End\":\"24:00\"}")!;
            Assert.Equal(new EndOfDayTime(9, 30), shift.Start);
            Assert.Equal(EndOfDayTime.EndOfDay, shift.End);
        }

        [Fact]
        public void Json_Nullable_RoundTripsNull()
        {
            var json = JsonSerializer.Serialize(new JsonShift { Start = new EndOfDayTime(9, 0) });
            Assert.Equal("{\"Start\":\"09:00\",\"End\":null}", json);
            Assert.Null(JsonSerializer.Deserialize<JsonShift>(json)!.End);
        }

        [Theory]
        [InlineData("930")]
        [InlineData("\"25:00\"")]
        [InlineData("true")]
        public void Json_InvalidValue_ThrowsJsonException(string json)
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EndOfDayTime>(json));
        }

        [Theory]
        [InlineData("+5:30")]
        [InlineData("-0:00")]
        [InlineData("05:+9")]
        [InlineData("1 :30")]
        [InlineData("05: 9")]
        [InlineData("٠٩:٣٠")]
        [InlineData("09:30:00")]
        [InlineData("09.30")]
        public void TryParse_NonDigitInput_ReturnsFalse(string input)
        {
            Assert.False(EndOfDayTime.TryParse(input, out _));
        }

        [Theory]
        [InlineData(" 09:30 ", 9, 30)]
        [InlineData("00:00", 0, 0)]
        [InlineData("24:00", 24, 0)]
        public void TryParse_ValidInput_ReturnsTrue(string input, int hour, int minute)
        {
            Assert.True(EndOfDayTime.TryParse(input, out var result));
            Assert.Equal(new EndOfDayTime(hour, minute), result);
        }

        // ── TimeOnly conversion ──────────────────────────────────────────

        [Fact]
        public void ToTimeOnly_ReturnsCorrectValue()
        {
            Assert.Equal(new TimeOnly(9, 30), new EndOfDayTime(9, 30).ToTimeOnly());
            Assert.Equal(new TimeOnly(0, 0), (TimeOnly)new EndOfDayTime(0, 0));
        }

        [Fact]
        public void ToTimeOnly_EndOfDay_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => EndOfDayTime.EndOfDay.ToTimeOnly());
            Assert.Throws<InvalidOperationException>(() => (TimeOnly)EndOfDayTime.EndOfDay);
        }

        [Fact]
        public void FromTimeOnly_ReturnsCorrectValue()
        {
            Assert.Equal(new EndOfDayTime(17, 45), EndOfDayTime.FromTimeOnly(new TimeOnly(17, 45)));
            Assert.Equal(new EndOfDayTime(17, 45), (EndOfDayTime)new TimeOnly(17, 45));
        }

        [Fact]
        public void FromTimeOnly_DiscardsSeconds()
        {
            Assert.Equal(new EndOfDayTime(23, 59), EndOfDayTime.FromTimeOnly(new TimeOnly(23, 59, 59, 999)));
        }

        // ── TimeSpan conversion ──────────────────────────────────────────

        [Fact]
        public void ToTimeSpan_ReturnsCorrectValue()
        {
            var t = new EndOfDayTime(9, 30);
            Assert.Equal(TimeSpan.FromMinutes(570), t.ToTimeSpan());
        }
    }
}