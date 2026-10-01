using System.Threading;
using System.Windows;
using EndOfDayTime.Wpf;
using Xunit;
using EodtCore = EndOfDayTime.Core;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace EndOfDayTime.Wpf.Tests
{
    public class EndOfDayTimeTextBoxTests
    {
        private static T RunOnSta<T>(Func<T> func)
        {
            T result = default!;
            Exception? ex = null;
            var thread = new Thread(() =>
            {
                try { result = func(); }
                catch (Exception e) { ex = e; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (ex != null) throw ex;
            return result;
        }

        private static void RunOnSta(Action action) =>
            RunOnSta<bool>(() => { action(); return true; });

        // ── Construction ─────────────────────────────────────────────────

        [Fact]
        public void Constructor_DefaultTimeValue_IsNull()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Assert.Null(control.TimeValue);
            });
        }

        [Fact]
        public void Constructor_MaxLength_IsFive()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                Assert.Equal(5, control.MaxLength);
            });
        }

        // ── TimeValue property ───────────────────────────────────────────

        [Fact]
        public void SetTimeValue_UpdatesText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
                Assert.Equal("09:30", control.Text);
            });
        }

        [Fact]
        public void SetTimeValue_EndOfDay_UpdatesText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = EodtCore.EndOfDayTime.EndOfDay;
                Assert.Equal("24:00", control.Text);
            });
        }

        [Fact]
        public void SetTimeValue_Null_TextIsEmpty()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
                control.TimeValue = null;
                Assert.Equal(string.Empty, control.Text);
            });
        }

        [Fact]
        public void SetTimeValue_Midnight_UpdatesText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(0, 0);
                Assert.Equal("00:00", control.Text);
                Assert.Equal(new EodtCore.EndOfDayTime(0, 0), control.TimeValue);
            });
        }

        [Fact]
        public void ClearText_SetsTimeValueToNull()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.TimeValue = new EodtCore.EndOfDayTime(9, 0);
                control.Text = string.Empty;
                Assert.Null(control.TimeValue);
            });
        }

        [Fact]
        public void Converter_Midnight_ConvertsToText()
        {
            var converter = new EndOfDayTimeConverter();
            var text = converter.Convert(
                new EodtCore.EndOfDayTime(0, 0), typeof(string), null!,
                System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal("00:00", text);
        }

        // ── Validate() ───────────────────────────────────────────────────

        [Fact]
        public void Validate_ValidText_ReturnsTrue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "09:30";
                Assert.True(control.Validate());
                Assert.True(control.IsValid);
            });
        }

        [Fact]
        public void Validate_ValidText_UpdatesTimeValue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "24:00";
                control.Validate();
                Assert.Equal(EodtCore.EndOfDayTime.EndOfDay, control.TimeValue);
            });
        }

        [Fact]
        public void Validate_InvalidText_ReturnsFalse()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "bad";
                Assert.False(control.Validate());
                Assert.False(control.IsValid);
            });
        }

        [Fact]
        public void Validate_EmptyText_ReturnsFalse()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = string.Empty;
                Assert.False(control.Validate());
                Assert.False(control.IsValid);
            });
        }

        [Fact]
        public void Validate_EndOfDay_ReturnsTrue()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "24:00";
                Assert.True(control.Validate());
                Assert.Equal(EodtCore.EndOfDayTime.EndOfDay, control.TimeValue);
            });
        }

        // ── TimeValueChanged event ───────────────────────────────────────

        [Fact]
        public void TimeValueChanged_Fires_WhenTimeValueSet()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                EodtCore.EndOfDayTime? received = null;
                control.TimeValueChanged += (s, e) => received = e.NewValue;
                control.TimeValue = new EodtCore.EndOfDayTime(9, 30);
                Assert.NotNull(received);
                Assert.Equal(new EodtCore.EndOfDayTime(9, 30), received!.Value);
            });
        }

        [Fact]
        public void TimeValueChanged_Fires_WhenUserEntersText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                var received = new List<EodtCore.EndOfDayTime?>();
                control.TimeValueChanged += (s, e) => received.Add(e.NewValue);
                control.Text = "09:30";
                Assert.Equal(new EodtCore.EndOfDayTime?[] { new EodtCore.EndOfDayTime(9, 30) }, received);
                Assert.Equal("09:30", control.Text);
            });
        }

        [Fact]
        public void TimeValueChanged_Fires_WhenUserClearsText()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "09:30";
                var received = new List<EodtCore.EndOfDayTime?>();
                control.TimeValueChanged += (s, e) => received.Add(e.NewValue);
                control.Text = string.Empty;
                Assert.Equal(new EodtCore.EndOfDayTime?[] { null }, received);
            });
        }

        [Fact]
        public void TimeValueChanged_DoesNotFire_WhenValueUnchanged()
        {
            RunOnSta(() =>
            {
                var control = new EndOfDayTimeTextBox();
                control.Text = "09:30";
                int count = 0;
                control.TimeValueChanged += (s, e) => count++;
                control.Validate();
                Assert.Equal(0, count);
            });
        }
    }
}